using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static UnityEditor.PlayerSettings;


namespace Toy_1
{

    public class BulletFiringInfo
    {
        public int repeatCount = 1;
        public float fireRate = 0.5f;
        public bool bIsClockwise = true;
        public bool bDoFireAtOnce = false;
    }

    public class BulletManager : MonoBehaviour
    {
        ObjectPool<BulletInfo> bulletPool;
        List<BulletInfo> bulletsForUpdating = new List<BulletInfo>();
        private bool mbIsGamePlaying = false;
        // Start is called before the first frame update
        void Start()
        {
            bulletPool = new ObjectPool<BulletInfo>(CreateBullet, OnTakeBulletFromPool, OnReturnBulletToPool, OnDestroyBullet,
                collectionCheck: true,
                defaultCapacity: 64);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public Transform srcTrans;
        public Transform dstTrans;
        public GameObject bulletIndicator;
        public BulletInfo bulletPrefab;
        public float bulletDistance = 2f;
        public float maxBulletSpreadAngle = 90f;
        public float bulletFireRate = 0.5f;
        public float bulletSpeed = 1f;

        public BulletInfo CreateBullet()
        {

            BulletInfo bullet = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation);
            bullet.SetPool(bulletPool);
            bulletsForUpdating.Add(bullet);
            return bullet;
        }
        public void OnTakeBulletFromPool(BulletInfo bulletInfo)
        {
            bulletInfo.gameObject.SetActive(true);
            bulletInfo.bIsReleased = false;

        }
        public void OnReturnBulletToPool(BulletInfo bulletInfo)
        {
            bulletInfo.gameObject.SetActive(false);
            bulletsForUpdating.Remove(bulletInfo);
        }
        public void OnDestroyBullet(BulletInfo bulletInfo)
        {
            Destroy(bulletInfo.gameObject);
        }


        [TestMethod(false)]
        public void TestSpawnBullet(int bulletCount)
        {
            SpawnBullet(srcTrans.position, dstTrans.position, bulletCount);
        }

        [TestMethod(false)]
        public void TestSpawnBulletWithRoutine(int bulletCount)
        {
            // SpawnBullet(srcTrans.position, dstTrans.position, bulletCount);
            StartCoroutine(SpawnBulletRoutine(srcTrans.position, dstTrans.position, bulletCount, 2));
        }

        public void SpawnBullet(Vector3 srcPos, Vector3 dstPos, int bulletCount)
        {
            var paramPos = dstPos - srcPos;
            paramPos.y = 0f;
            paramPos.Normalize();
            var tempRot = Quaternion.LookRotation(dstPos - srcPos, Vector3.up);
            var tempRot2 = Quaternion.LookRotation(paramPos, Vector3.up);
            var tempDistance = Vector3.Distance(srcPos, dstPos);
            var tempPos = new Vector3(0, 0, 0);

            // tempRot.y
            var tempEulerAngle = tempRot.eulerAngles;
            
            // tempRot.z += bulletDistance;

            // tempPos.y = tempDistance * Mathf.Cos(tempPos.y * Mathf.Deg2Rad);
            

            // sin is + in 1st and 2nd quadrant
            // cos is + in 1st and 4th quadrant

            tempEulerAngle.y += -maxBulletSpreadAngle / 2;
            for (int i = 0; i < bulletCount; i++)
            {
                
                tempPos.x = tempDistance * Mathf.Sin(tempEulerAngle.y * Mathf.Deg2Rad);
                tempPos.z = tempDistance * Mathf.Cos(tempEulerAngle.y * Mathf.Deg2Rad);
                var tempObject = Instantiate(bulletIndicator, tempPos, tempRot);
                tempObject.name = "Bullet_" + i;

                Debug.Log(" Bullet: " + i + "'s "
                    + " temp pos is " + tempPos
                    + " temp angle is " + tempEulerAngle.y);

                // tempEulerAngle.y += 360f / bulletCount;
                // at one of second, bullet is on middle
                var tempCount = bulletCount / 2;
                
                tempEulerAngle.y += maxBulletSpreadAngle / bulletCount;

                //if(i == bulletCount / 2 )
                //{
                //    tempDistance += bulletDistance;
                //}

            }

            Debug.Log("tempPos: " + tempPos
                + " tempRot: " + tempRot
                + " src pos: " + srcPos
                + " dst pos: " + dstPos
                + " tempRot2: " + tempRot2
                + " paramPos: " + paramPos
                + " tempDist: " + tempDistance
                + " parampos mag: " + paramPos.magnitude);


            Debug.Log("rot x is " + tempRot.x
                + " rot y is " + tempRot.y
                + " rot z is " + tempRot.z);


            // var tempRot = Mathf.Cos()
        }

        public IEnumerator SpawnBulletRoutine(Vector3 srcPos, Vector3 dstPos, int bulletCount, int loopCount)
        {
            var paramPos = dstPos - srcPos;
            paramPos.y = 0f;
            paramPos.Normalize();
            var tempRot = Quaternion.LookRotation(dstPos - srcPos, Vector3.up);
            var tempRot2 = Quaternion.LookRotation(paramPos, Vector3.up);
            var tempDistance = Vector3.Distance(srcPos, dstPos);
            var tempPos = new Vector3(0, 0, 0);
            var tempEulerAngle = tempRot.eulerAngles;

            // 
            // mbIsGamePlaying = true;
            SetGamePlaying(true);
            StartCoroutine(UpdateEveryBulletTick());

            // temp
            // Invoke("StopGamePlaying", 10f);
            //


            // if cone to circle shape
            tempEulerAngle.y += -maxBulletSpreadAngle / 2;
            for (int i = 0; i < loopCount; i++)
            {
                for (int k = 0; k < bulletCount; k++)
                {
                    tempPos.x = tempDistance * Mathf.Sin(tempEulerAngle.y * Mathf.Deg2Rad);
                    tempPos.z = tempDistance * Mathf.Cos(tempEulerAngle.y * Mathf.Deg2Rad);
                    // var tempObject = Instantiate(bulletIndicator, srcPos, tempRot);
                    var tempObject = bulletPool.Get().gameObject;
                    tempObject.name = "Bullet_" + k;
                    BulletInfo tempComp;
                    tempObject.TryGetComponent(out tempComp);

                    tempObject.transform.position = srcPos;

                    if(tempComp != null)
                    {
                        tempComp.progressDirection = tempPos;
                        tempComp.speed = bulletSpeed;
                    }
                    else
                    {
                        tempComp = tempObject.AddComponent<BulletInfo>();
                        tempComp.progressDirection = tempPos;
                        tempComp.speed = bulletSpeed;

                    }
                    

                    Debug.Log(" Bullet: " + k + "'s "
                        + " temp pos is " + tempPos
                        + " temp angle is " + tempEulerAngle.y
                        + " bullet position " + tempComp.transform.position);

                    tempEulerAngle.y += maxBulletSpreadAngle / bulletCount;
                    yield return new WaitForSeconds(bulletFireRate);

                }
            }

            //

            yield return null;


        }

        public void SetGamePlaying(bool bIsPlaying)
        {
            mbIsGamePlaying = bIsPlaying;
        }

        public void StopGamePlaying()
        {
            mbIsGamePlaying = false;
        }

        public IEnumerator UpdateEveryBulletTick()
        {
            while(mbIsGamePlaying)
            {
                //if(bulletPool.CountActive == 0)
                //{
                //    yield break;
                //    // yield return null;
                //}
                for (int i = 0; i < bulletsForUpdating.Count; i++)
                {
                    var tempBullet = bulletsForUpdating[i];
                    if (tempBullet.gameObject.activeSelf == false)
                    {
                        continue;
                    }
                    tempBullet.transform.position += Time.deltaTime * tempBullet.progressDirection * tempBullet.speed;
                    // tempBullet.transform.position +=  tempBullet.progressDirection * tempBullet.speed;
                    if (Vector3.Distance(tempBullet.transform.position, dstTrans.position) < 0.1f)
                    {
                        tempBullet.Disable();
                    }
                }
                yield return null;
                // yield return new WaitForSeconds(2f);
            }
        }

    }

}
