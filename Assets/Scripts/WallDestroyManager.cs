using System.Collections.Generic;
using UnityEngine;

// Object Pool System for Wall Entities
public class WallEntityPool
{
    private Queue<WallEntity> availableWalls = new Queue<WallEntity>();
    private List<WallEntity> allPooledWalls = new List<WallEntity>();
    private Transform poolParent;

    public WallEntityPool(Transform parent = null)
    {
        poolParent = parent;
        if (poolParent == null)
        {
            GameObject poolContainer = new GameObject("WallPool");
            poolParent = poolContainer.transform;
        }
    }

    public void ReturnToPool(WallEntity wallEntity)
    {
        if (wallEntity == null) return;

        // Reset wall state
        wallEntity.ResetToPoolState();

        // Deactivate and move to pool parent
        wallEntity.gameObject.SetActive(false);
        wallEntity.transform.SetParent(poolParent);

        // Add to available queue
        if (!availableWalls.Contains(wallEntity))
        {
            availableWalls.Enqueue(wallEntity);
        }
    }

    public WallEntity GetFromPool()
    {
        if (availableWalls.Count > 0)
        {
            WallEntity wall = availableWalls.Dequeue();
            wall.gameObject.SetActive(true);
            return wall;
        }
        return null;
    }

    public int GetAvailableCount()
    {
        return availableWalls.Count;
    }

    public int GetTotalPooledCount()
    {
        return allPooledWalls.Count;
    }

    public void PrewarmPool(GameObject wallPrefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject wallObj = Object.Instantiate(wallPrefab, poolParent);
            WallEntity wallEntity = wallObj.GetComponent<WallEntity>();
            if (wallEntity != null)
            {
                allPooledWalls.Add(wallEntity);
                ReturnToPool(wallEntity);
            }
        }
    }

    public void ClearPool()
    {
        foreach (WallEntity wall in allPooledWalls)
        {
            if (wall != null && wall.gameObject != null)
            {
                Object.Destroy(wall.gameObject);
            }
        }
        allPooledWalls.Clear();
        availableWalls.Clear();
    }
}

public class WallDestroyManager : MonoBehaviour
{
    [Header("Destruction Settings")]
    public float destructionRadius = 0.5f;
    public bool allowDestroyLandedWalls = true;
    public bool allowDestroyFallingWalls = true;
    public bool useRaycastDetection = true;
    public bool enableDebugLog = true;

    [Header("Pool Settings")]
    public bool useObjectPooling = true;
    public int initialPoolSize = 50;
    public GameObject wallPrefabForPool; // Reference prefab for pool prewarming

    [Header("Visual Effects")]
    public GameObject destructionEffect;
    public float effectDuration = 1f;

    [Header("References")]
    public WallCreateManager wallCreateManager;

    // Object Pool System
    private WallEntityPool wallPool;
    private Transform poolContainer;

    void Start()
    {
        // WallCreateManager 자동 찾기
        if (wallCreateManager == null)
        {
            wallCreateManager = FindFirstObjectByType<WallCreateManager>();
        }

        // Initialize Object Pool
        if (useObjectPooling)
        {
            InitializePool();
        }
    }

    private void InitializePool()
    {
        // Create pool container
        GameObject poolObj = new GameObject("WallPool");
        poolObj.transform.SetParent(this.transform);
        poolContainer = poolObj.transform;

        // Initialize pool system
        wallPool = new WallEntityPool(poolContainer);

        // Prewarm pool if prefab is provided
        if (wallPrefabForPool != null && initialPoolSize > 0)
        {
            wallPool.PrewarmPool(wallPrefabForPool, initialPoolSize);
            Debug.Log($"Wall pool prewarmed with {initialPoolSize} objects");
        }
    }

    public bool DestroyWallAtPosition(Vector3 targetPosition)
    {
        WallEntity targetWall = null;

        if (useRaycastDetection)
        {
            targetWall = FindWallByRaycast(targetPosition);
        }
        else
        {
            targetWall = FindClosestWallAtPosition(targetPosition);
        }

        if (targetWall != null)
        {
            return DestroyWallEntity(targetWall);
        }

        Debug.Log($"좌표 {targetPosition}에서 파괴할 벽을 찾을 수 없습니다.");
        return false;
    }

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
    /// 특정 WallEntity를 파괴합니다 (Pool 시스템 사용)
    /// </summary>
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

        // Object Pool 사용 여부에 따라 처리
        if (useObjectPooling && wallPool != null)
        {
            // Pool로 반환
            wallPool.ReturnToPool(wallEntity);

            if (enableDebugLog)
            {
                Debug.Log($"Wall returned to pool. Available: {wallPool.GetAvailableCount()}/{wallPool.GetTotalPooledCount()}");
            }
        }
        else
        {
            // 기존 방식: GameObject 파괴
            Destroy(wallEntity.gameObject);
        }

        return true;
    }

    /// <summary>
    /// Pool에서 Wall Entity를 가져옵니다
    /// </summary>
    public WallEntity GetWallFromPool()
    {
        if (!useObjectPooling || wallPool == null)
        {
            Debug.LogWarning("Object pooling is disabled or not initialized");
            return null;
        }

        WallEntity wall = wallPool.GetFromPool();

        if (wall != null && enableDebugLog)
        {
            Debug.Log($"Wall retrieved from pool. Available: {wallPool.GetAvailableCount()}/{wallPool.GetTotalPooledCount()}");
        }

        return wall;
    }

    /// <summary>
    /// Pool 상태 정보를 반환합니다
    /// </summary>
    public (int available, int total) GetPoolStatus()
    {
        if (wallPool == null) return (0, 0);
        return (wallPool.GetAvailableCount(), wallPool.GetTotalPooledCount());
    }

    // 기존 메서드들은 동일하게 유지
    private WallEntity FindWallByRaycast(Vector3 targetPosition)
    {
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

        if (enableDebugLog)
        {
            Debug.Log("OverlapPoint failed, trying distance-based detection");
        }

        return FindClosestWallAtPosition(targetPosition);
    }

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

    private WallEntity FindExactWallAtGridPosition(Vector3 gridPosition)
    {
        if (wallCreateManager == null || wallCreateManager.storedWallEntities.Count == 0)
            return null;

        foreach (WallEntity wall in wallCreateManager.storedWallEntities)
        {
            if (wall == null) continue;

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

    private bool CanDestroyWall(WallEntity wall)
    {
        if (wall == null) return false;

        if (allowDestroyLandedWalls && wall.IsLanded())
        {
            return true;
        }

        if (allowDestroyFallingWalls && !wall.IsLanded())
        {
            return true;
        }

        return false;
    }

    private void PlayDestructionEffect(Vector3 position)
    {
        if (destructionEffect != null)
        {
            GameObject effect = Instantiate(destructionEffect, position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            DestroyWallAtPosition(mouseWorldPos);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Debug.Log("오른쪽 클릭: 범위 파괴는 비활성화되어 있습니다.");
            DestroyWallsInArea(mouseWorldPos, destructionRadius * 2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, destructionRadius);
    }

    void OnDestroy()
    {
        // Clean up pool when manager is destroyed
        if (wallPool != null)
        {
            wallPool.ClearPool();
        }
    }
}
