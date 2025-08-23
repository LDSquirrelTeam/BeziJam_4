using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


public enum eWallSize
{
    // Small = 1, // 1x1
    Medium = 2, // 2x2
    Large = 4, // 4x4
    ExtraLarge = 8 // 8x8
}

public class WallCreateManager : MonoBehaviour
{
    public WallEntity wallPrefab;
    // stl::vector
    // stl::map
    // queue.. stack?...
    public List<WallEntity> storedWallEntities = new List<WallEntity>();
    public float tickTime = 0.1f; // seconds
    public float currentTime = 0f; // seconds
    public float fallingSpeed = 0.1f; // how much the wall moves down per tick


    private void Update()
    {
        // 60 fps
        // 1 second = 60 frames
        // 1/60 = 0.0166667 seconds per frame
        // per tickTime, execute below.
        //currentTime += Time.deltaTime;
        //if (currentTime >= tickTime)
        //{
        //    TickForWallsMoving();
        //    currentTime = 0f; // reset currentTime
        //}
        TickForWallsMoving();
    }

    public void SpawnWall(int paramSize)
    {
        // create
        // 2x2
        // 4x4
        // 8x8
        // with 1x1 wall prefab
        //GameObject wall = Instantiate(wallPrefab, new Vector3(0, 10, 0), Quaternion.identity);
        // if 2x2
        // (1,0), (1,1), 
        // (0,0), (0,1)

        CreateWall(paramSize, paramSize);
        
        //switch (paramSize)
        //{
        //    case eWallSize.Medium:
        //        CreateWall(2,2);
        //        break;
        //    case eWallSize.Large:
        //        CreateWall(4,4);
        //        break;
        //    case eWallSize.ExtraLarge:
        //        CreateWall(8,8);
        //        break;
        //    default:
        //        Debug.LogError("Invalid wall size");
        //        break;
        //}



    }

    private void CreateWall(int v1, int v2)
    {
        // Calculate center offset to position grid around origin
        float offsetX = -(v1 - 1) / 2.0f;
        float offsetY = -(v2 - 1) / 2.0f;

        for (int i = 0; i < v1; i++)
        {
            for (int j = 0; j < v2; j++)
            {
                // Create wall prefab with center-based positioning
                Vector3 position = new Vector3(i + offsetX, j + offsetY, 0);
                WallEntity wall = Instantiate(wallPrefab, position, Quaternion.identity);
                wall.transform.localScale = new Vector3(1, 1, 1);

                storedWallEntities.Add(wall);

            }
        }
    }


    public void CheckAttackHitWall()
    { 

    }

    public void BreakWall(int x, int y)
    { 
        // character, attack
        // ?

        // 2x2 
        // x,y -> gameobject -> setactive(false)

        // 1. 캐릭터가 공격을 한다. -> collider ->

        // 2. 공격 범위 내에 벽이 있는지 판정한다. ->
        
        // 3. 벽을 부순다. -> 

        // char -> attack 
        // attack area or range 
        //  
        // -> breakwall()
        //

    }

    public void TickForWallsMoving()
    {
        
        if(storedWallEntities.Count <= 0)
        {
            // Debug.LogWarning("No walls to move.");
            return;
        }
        // per tickTime, execute below.
        for (int i = 0; i < storedWallEntities.Count; i++)
        {
            var currentEntity = storedWallEntities[i];
            // please forking copilot, make currentEntity's transform position y -= 0.1f;
            if (currentEntity != null)
            {
                currentEntity.transform.position += new Vector3(0, -1f * fallingSpeed, 0);
            }
            else
            {
                Debug.LogWarning("WallEntity is null at index " + i);
            }
        }
    }


}
