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


    // 1. pool���� �������� GetFromPool
    // 2. pool�� �ֱ� ReturnToPool

    private void Start()
    {
        // createFunc -> �޸� �Ҵ� �� Ǯ�� ���� ��
        // actionOnGet -> Ǯ���� ���� ��
        // actionOnRelease -> Ǯ�� ���� ��

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
        // 1. ������ �����͸� ������
        // 1.1. Ŭ���̾�Ʈ�� ���̶� ������ �� -> A �Լ� ȣ�� -> 
        // 1.2. ���(����, ����, ����..���) -> B, C, D...�Լ� ȣ��...

        
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
