using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletInfo : MonoBehaviour
{

    public Vector3 progressDirection; // direction of the bullet, deltatime * speed
    public float speed; // speed of the bullet
    private ObjectPool<BulletInfo> mPool;
    public bool bIsReleased = false;
    public float autoDestroyTime = 5f;

    private const string DISABLE_METHOD_NAME = "Disable";


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Invoke(DISABLE_METHOD_NAME, autoDestroyTime);
    }

    public void SetPool(ObjectPool<BulletInfo> paramPool)
    {
        mPool = paramPool;
    }

    public void Disable()
    {
        if (bIsReleased)
        {
            return;
        }
        if(mPool == null)
        {
            Destroy(gameObject);
            return;
        }
        CancelInvoke(DISABLE_METHOD_NAME);
        //rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
        // how to check this object is already released to mPool?
        bIsReleased = true;
        mPool.Release(this);
    }

}
