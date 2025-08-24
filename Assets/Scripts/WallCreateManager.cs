using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eWallSize
{
    Medium = 2,     // 2x2
    Large = 4,      // 4x4
    ExtraLarge = 8  // 8x8
}

public class WallCreateManager : MonoBehaviour
{
    public WallEntity wallPrefab;
    public List<WallEntity> storedWallEntities = new List<WallEntity>();
    public float tickTime = 0.1f; // seconds
    public float currentTime = 0f; // seconds
    public float fallingSpeed = 0.1f; // how much the wall moves down per tick
    
    [Header("Auto Remove Settings")]
    public bool autoRemoveAfterLanding = false; // 착지 후 자동 제거 기능
    public float autoRemoveDelay = 2f; // 자동 제거 딜레이
    
    [Header("Floor Settings")]
    public GameObject floorObject; // floor GameObject 참조
    private float floorTopY; // floor 상단 Y 좌표

    private bool isClearing = false; // 중복 실행 방지

    private void Update()
    {
        TickForWallsMoving();
    }
    
    void Start()
    {
        if (floorObject == null)
        {
            floorObject = GameObject.Find("floor");
        }
        
        CalculateFloorTopY();
    }
    
    void CalculateFloorTopY()
    {
        if (floorObject != null)
        {
            Renderer floorRenderer = floorObject.GetComponent<Renderer>();
            if (floorRenderer != null)
            {
                floorTopY = floorRenderer.bounds.max.y;
                Debug.Log($"Floor top Y calculated: {floorTopY}");
            }
            else
            {
                Transform floorTransform = floorObject.transform;
                float floorHeight = floorTransform.localScale.y;
                floorTopY = floorTransform.position.y + (floorHeight / 2f);
                Debug.Log($"Floor top Y calculated from transform: {floorTopY}");
            }
        }
        else
        {
            floorTopY = -4f;
            Debug.LogWarning("Floor object not found, using default floor Y: " + floorTopY);
        }
    }

    public void SpawnWall(int paramSize)
    {
        CreateWall(paramSize, paramSize);
    }

    private void CreateWall(int v1, int v2)
    {
        float offsetX = -(v1 - 1) / 2.0f;
        float offsetY = -(v2 - 1) / 2.0f;

        for (int i = 0; i < v1; i++)
        {
            for (int j = 0; j < v2; j++)
            {
                Vector3 position = new Vector3(i + offsetX, j + offsetY, 0);
                WallEntity wall = Instantiate(wallPrefab, position, Quaternion.identity);
                wall.transform.localScale = new Vector3(1, 1, 1);

                storedWallEntities.Add(wall);
            }
        }
    }

    public void TickForWallsMoving()
    {
        if (storedWallEntities.Count <= 0 || isClearing)
            return;
        
        for (int i = 0; i < storedWallEntities.Count; i++)
        {
            var currentEntity = storedWallEntities[i];
            
            if (currentEntity != null)
            {
                if (!currentEntity.IsLanded())
                {
                    Vector3 nextPosition = currentEntity.transform.position + new Vector3(0, -1f * fallingSpeed, 0);
                    Vector3 originalPos = currentEntity.transform.position;

                    float wallBottomY = nextPosition.y - (currentEntity.transform.localScale.y / 2f);

                    bool hasCollision = false;

                    if (wallBottomY <= floorTopY)
                    {
                        hasCollision = true;
                    }

                    Collider2D currentCollider = currentEntity.GetComponent<Collider2D>();
                    foreach (var otherEntity in storedWallEntities)
                    {
                        if (otherEntity != currentEntity && otherEntity != null && otherEntity.IsLanded())
                        {
                            Collider2D otherCollider = otherEntity.GetComponent<Collider2D>();
                            if (currentCollider.bounds.Intersects(otherCollider.bounds))
                            {
                                hasCollision = true;
                                break;
                            }
                        }
                    }

                    if (hasCollision)
                    {
                        currentEntity.transform.position = originalPos;
                        currentEntity.TriggerLanding();
                    }
                    else
                    {
                        currentEntity.transform.position = nextPosition;
                    }
                }
            }
        }

        // 자동 제거가 활성화된 경우에만 모든 블록 착지 확인
        if (autoRemoveAfterLanding)
        {
            CheckAllBlocksLanded();
        }
    }

    private void CheckAllBlocksLanded()
    {
        if (storedWallEntities.Count == 0) return;

        bool allLanded = true;
        foreach (var wall in storedWallEntities)
        {
            if (wall != null && !wall.IsLanded())
            {
                allLanded = false;
                break;
            }
        }

        if (allLanded && !isClearing)
        {
            Debug.Log($"모든 블록이 착지했습니다. {autoRemoveDelay}초 후 제거됩니다.");
            StartCoroutine(RemoveAllBlocksAfterDelay(autoRemoveDelay));
        }
    }

    private IEnumerator RemoveAllBlocksAfterDelay(float delay)
    {
        isClearing = true;
        yield return new WaitForSeconds(delay);

        foreach (var wall in storedWallEntities)
        {
            if (wall != null)
                Destroy(wall.gameObject);
        }
        storedWallEntities.Clear();
        isClearing = false;
    }
    
    /// <summary>
    /// 수동으로 모든 벽을 제거합니다
    /// </summary>
    public void ManuallyRemoveAllWalls()
    {
        foreach (var wall in storedWallEntities)
        {
            if (wall != null)
                Destroy(wall.gameObject);
        }
        storedWallEntities.Clear();
        isClearing = false;
        Debug.Log("모든 벽이 수동으로 제거되었습니다.");
    }
    
    /// <summary>
    /// 착지한 벽들만 제거합니다
    /// </summary>
    public void RemoveLandedWalls()
    {
        for (int i = storedWallEntities.Count - 1; i >= 0; i--)
        {
            var wall = storedWallEntities[i];
            if (wall != null && wall.IsLanded())
            {
                Destroy(wall.gameObject);
                storedWallEntities.RemoveAt(i);
            }
        }
        Debug.Log("착지한 벽들이 제거되었습니다.");
    }
}
