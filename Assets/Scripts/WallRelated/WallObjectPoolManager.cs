using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WallObjectPoolManager : MonoBehaviour
{
    // List<ObjectPool<WallEntity>> wallPool = new List<ObjectPool<WallEntity>>();
    ObjectPool<WallEntity> wallPool;

    Func<float> functionForFloat;
    Action action;


    // 1. pool에서 가져오기 GetFromPool
    // 2. pool에 넣기 ReturnToPool

    private void Start()
    {
        // createFunc -> 메모리 할당 후 풀에 넣을 때
        // actionOnGet -> 풀에서 꺼낼 때
        // actionOnRelease -> 풀에 넣을 때

        wallPool = new ObjectPool<WallEntity>(
            createFunc: OnAllocateToMemoryAndPool,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyObjectFromPool
            );

        //wallPool = new ObjectPool<WallEntity>(
        //    createFunc: () =>
        //    {
        //        GameObject wallObj = new GameObject("WallEntity");
        //        WallEntity wallEntity = wallObj.AddComponent<WallEntity>();
        //        // wallEntity.Initialize();
        //        return wallEntity;
        //    },
        //    actionOnGet: (wallEntity) =>
        //    {
        //        wallEntity.gameObject.SetActive(true);
        //        wallEntity.


        //    },
        //    actionOnRelease: (wallEntity) =>
        //    {
        //        wallEntity.gameObject.SetActive(false);
        //    },
        //    actionOnDestroy: (wallEntity) =>
        //    {
        //        Destroy(wallEntity.gameObject);
        //    },
        //    collectionCheck: false,
        //    defaultCapacity: 10,
        //    maxSize: 20
        //);
    }

    public void Initialize()
    {
        // 1. 웹에서 데이터를 가져와
        // 1.1. 클라이언트가 웹이랑 소통을 함 -> A 함수 호출 -> 
        // 1.2. 결과(성공, 실패, 에러..등등) -> B, C, D...함수 호출...

        
    }

    public WallEntity OnAllocateToMemoryAndPool()
    {
        return null;
    }
    public void OnGetFromPool(WallEntity wallEntity)
    {
        // return null;
    }
    public void OnReturnToPool(WallEntity wallEntity)
    {

    }
    public void OnDestroyObjectFromPool(WallEntity wallEntity)
    {

    }

}
