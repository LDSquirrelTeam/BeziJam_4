using System.Collections.Generic;
using UnityEngine;

public class WallDestroyManager : MonoBehaviour
{
    [Header("Destruction Settings")]
    public float destructionRadius = 0.5f; // 파괴 범위 (더 넓게)
    public bool allowDestroyLandedWalls = true; // 착지한 벽만 파괴 가능
    public bool allowDestroyFallingWalls = true; // 떨어지는 벽도 파괴 가능
    public bool useRaycastDetection = true; // Raycast를 사용한 정확한 감지
    public bool enableDebugLog = true; // 디버그 로그 활성화
    
    [Header("Visual Effects")]
    public GameObject destructionEffect; // 파괴 효과 프리팹 (선택사항)
    public float effectDuration = 1f;
    
    [Header("References")]
    public WallCreateManager wallCreateManager; // WallCreateManager 참조
    
    void Start()
    {
        // WallCreateManager 자동 찾기
        if (wallCreateManager == null)
        {
            wallCreateManager = FindFirstObjectByType<WallCreateManager>();
        }
    }
    
    /// <summary>
    /// 특정 좌표에서 가장 가까운 Wall Entity를 파괴합니다
    /// </summary>
    /// <param name="targetPosition">파괴할 좌표</param>
    /// <returns>파괴 성공 여부</returns>
    public bool DestroyWallAtPosition(Vector3 targetPosition)
    {
        WallEntity targetWall = null;
        
        if (useRaycastDetection)
        {
            // Raycast를 사용한 정확한 감지
            targetWall = FindWallByRaycast(targetPosition);
        }
        else
        {
            // 기존 방식: 가장 가까운 벽 찾기
            targetWall = FindClosestWallAtPosition(targetPosition);
        }
        
        if (targetWall != null)
        {
            return DestroyWallEntity(targetWall);
        }
        
        Debug.Log($"좌표 {targetPosition}에서 파괴할 벽을 찾을 수 없습니다.");
        return false;
    }
    
    /// <summary>
    /// 특정 좌표 범위 내의 모든 Wall Entity를 파괴합니다
    /// </summary>
    /// <param name="targetPosition">파괴할 중심 좌표</param>
    /// <param name="radius">파괴 반경</param>
    /// <returns>파괴된 벽의 개수</returns>
    public int DestroyWallsInArea(Vector3 targetPosition, float radius)
    {
        List<WallEntity> wallsToDestroy = FindWallsInArea(targetPosition, radius);
        int destroyedCount = 0;
        
        foreach (WallEntity wall in wallsToDestroy)
        {
            if (DestroyWallEntity(wall))
            {
                destroyedCount++;
            }
        }
        
        Debug.Log($"좌표 {targetPosition} 반경 {radius} 내에서 {destroyedCount}개의 벽을 파괴했습니다.");
        return destroyedCount;
    }
    
    /// <summary>
    /// 특정 Grid 좌표의 Wall Entity를 파괴합니다 (정확한 그리드 위치)
    /// </summary>
    /// <param name="gridX">Grid X 좌표</param>
    /// <param name="gridY">Grid Y 좌표</param>
    /// <returns>파괴 성공 여부</returns>
    public bool DestroyWallAtGridPosition(int gridX, int gridY)
    {
        Vector3 gridPosition = new Vector3(gridX, gridY, 0);
        WallEntity exactWall = FindExactWallAtGridPosition(gridPosition);
        
        if (exactWall != null)
        {
            return DestroyWallEntity(exactWall);
        }
        
        Debug.Log($"Grid 좌표 ({gridX}, {gridY})에서 벽을 찾을 수 없습니다.");
        return false;
    }
    
    /// <summary>
    /// 특정 WallEntity를 파괴합니다
    /// </summary>
    /// <param name="wallEntity">파괴할 WallEntity</param>
    /// <returns>파괴 성공 여부</returns>
    public bool DestroyWallEntity(WallEntity wallEntity)
    {
        if (wallEntity == null) return false;
        
        // 파괴 조건 확인
        if (!CanDestroyWall(wallEntity))
        {
            Debug.Log($"벽 {wallEntity.name}을 파괴할 수 없습니다. (조건 미충족)");
            return false;
        }
        
        // 파괴 효과 재생
        PlayDestructionEffect(wallEntity.transform.position);
        
        // WallCreateManager의 리스트에서 제거
        if (wallCreateManager != null)
        {
            wallCreateManager.storedWallEntities.Remove(wallEntity);
        }
        
        Debug.Log($"벽 {wallEntity.name}이 파괴되었습니다.");
        
        // GameObject 파괴
        Destroy(wallEntity.gameObject);
        
        return true;
    }
    
    /// <summary>
    /// Raycast를 사용하여 정확히 클릭한 벽을 찾습니다
    /// </summary>
    private WallEntity FindWallByRaycast(Vector3 targetPosition)
    {
        // 2D OverlapPoint를 사용한 감지 (더 확실함)
        Collider2D[] colliders = Physics2D.OverlapPointAll(targetPosition);
        
        if (enableDebugLog)
        {
            Debug.Log($"OverlapPoint at {targetPosition}: Found {colliders.Length} colliders");
        }
        
        foreach (Collider2D collider in colliders)
        {
            WallEntity wall = collider.GetComponent<WallEntity>();
            if (wall != null)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"Found wall: {wall.name}, IsLanded: {wall.IsLanded()}, Position: {wall.transform.position}");
                }
                
                if (CanDestroyWall(wall))
                {
                    if (enableDebugLog)
                    {
                        Debug.Log($"Wall {wall.name} can be destroyed!");
                    }
                    return wall;
                }
                else
                {
                    if (enableDebugLog)
                    {
                        Debug.Log($"Wall {wall.name} cannot be destroyed. Landed: {wall.IsLanded()}, AllowLanded: {allowDestroyLandedWalls}, AllowFalling: {allowDestroyFallingWalls}");
                    }
                }
            }
        }
        
        // OverlapPoint가 실패하면 거리 기반 방식 사용
        if (enableDebugLog)
        {
            Debug.Log("OverlapPoint failed, trying distance-based detection");
        }
        
        return FindClosestWallAtPosition(targetPosition);
    }
    
    /// <summary>
    /// 특정 좌표에서 가장 가까운 벽을 찾습니다 (기존 방식)
    /// </summary>
    private WallEntity FindClosestWallAtPosition(Vector3 targetPosition)
    {
        if (wallCreateManager == null || wallCreateManager.storedWallEntities.Count == 0)
            return null;
        
        WallEntity closestWall = null;
        float closestDistance = float.MaxValue;
        
        if (enableDebugLog)
        {
            Debug.Log($"Distance-based search at {targetPosition}, checking {wallCreateManager.storedWallEntities.Count} walls");
        }
        
        foreach (WallEntity wall in wallCreateManager.storedWallEntities)
        {
            if (wall == null) continue;
            
            float distance = Vector3.Distance(wall.transform.position, targetPosition);
            
            if (enableDebugLog)
            {
                Debug.Log($"Wall {wall.name}: distance={distance:F3}, landed={wall.IsLanded()}, position={wall.transform.position}");
            }
            
            if (distance <= destructionRadius && distance < closestDistance)
            {
                if (CanDestroyWall(wall))
                {
                    closestWall = wall;
                    closestDistance = distance;
                    
                    if (enableDebugLog)
                    {
                        Debug.Log($"New closest wall: {wall.name}, distance: {distance:F3}");
                    }
                }
                else
                {
                    if (enableDebugLog)
                    {
                        Debug.Log($"Wall {wall.name} found but cannot be destroyed");
                    }
                }
            }
        }
        
        if (enableDebugLog && closestWall == null)
        {
            Debug.Log($"No wall found within destruction radius {destructionRadius}");
        }
        
        return closestWall;
    }
    
    /// <summary>
    /// 특정 범위 내의 모든 벽을 찾습니다
    /// </summary>
    private List<WallEntity> FindWallsInArea(Vector3 centerPosition, float radius)
    {
        List<WallEntity> wallsInArea = new List<WallEntity>();
        
        if (wallCreateManager == null || wallCreateManager.storedWallEntities.Count == 0)
            return wallsInArea;
        
        foreach (WallEntity wall in wallCreateManager.storedWallEntities)
        {
            if (wall == null) continue;
            
            float distance = Vector3.Distance(wall.transform.position, centerPosition);
            
            if (distance <= radius && CanDestroyWall(wall))
            {
                wallsInArea.Add(wall);
            }
        }
        
        return wallsInArea;
    }
    
    /// <summary>
    /// 정확한 그리드 좌표의 벽을 찾습니다
    /// </summary>
    private WallEntity FindExactWallAtGridPosition(Vector3 gridPosition)
    {
        if (wallCreateManager == null || wallCreateManager.storedWallEntities.Count == 0)
            return null;
        
        foreach (WallEntity wall in wallCreateManager.storedWallEntities)
        {
            if (wall == null) continue;
            
            // 정확한 위치 비교 (소수점 오차 고려)
            Vector3 wallPos = wall.transform.position;
            if (Mathf.Approximately(wallPos.x, gridPosition.x) && 
                Mathf.Approximately(wallPos.y, gridPosition.y))
            {
                if (CanDestroyWall(wall))
                {
                    return wall;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 해당 벽이 파괴 가능한지 확인합니다
    /// </summary>
    private bool CanDestroyWall(WallEntity wall)
    {
        if (wall == null) return false;
        
        // 착지한 벽만 파괴 가능한 경우
        if (allowDestroyLandedWalls && wall.IsLanded())
        {
            return true;
        }
        
        // 떨어지는 벽도 파괴 가능한 경우
        if (allowDestroyFallingWalls && !wall.IsLanded())
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 파괴 효과를 재생합니다
    /// </summary>
    private void PlayDestructionEffect(Vector3 position)
    {
        if (destructionEffect != null)
        {
            GameObject effect = Instantiate(destructionEffect, position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }
    
    /// <summary>
    /// 마우스 클릭으로 벽을 파괴하는 예제 메서드
    /// </summary>
    void Update()
    {
        // 마우스 왼쪽 클릭으로 정확한 벽 파괴
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // 2D이므로 Z는 0으로 설정
            
            DestroyWallAtPosition(mouseWorldPos);
        }
        
        // 마우스 오른쪽 클릭으로 범위 파괴 (비활성화 - 실수 방지)
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("오른쪽 클릭: 범위 파괴는 비활성화되어 있습니다.");
            // DestroyWallsInArea(mouseWorldPos, destructionRadius * 2f);
        }
    }
    
    // 디버그용 Gizmo 그리기
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, destructionRadius);
    }
}