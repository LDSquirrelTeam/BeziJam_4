using UnityEngine;

public class TestCreateWall : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallSpeed = 5f;

    [Header("Respawn Settings")]
    public GameObject prefabToSpawn;
    private GameObject firstObject;
    public Vector3 spawnPosition = new Vector3(0, 10f, 0);
    public int iterationCount = 0;

    void Start()
    {
        firstObject = SpawnNewObject();
        if (firstObject != null)
            firstObject.transform.position = new Vector3(transform.position.x, 10f, transform.position.z);
    }

    void Update()
    {
        if (firstObject == null)
        {
            firstObject = SpawnNewObject();
        }
        firstObject.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (firstObject.transform.position.y <= 0f)
        {
            // SpawnNewObject();
            Destroy(firstObject);
        }
    }

    GameObject SpawnNewObject()
    {
        if (prefabToSpawn != null)
        {
            iterationCount++;
            GameObject result = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            result.name = $"FallingObject_{iterationCount}";
            // 풀에서 가져와서 쓰려면
            // var tempObject = wallPool.Get().gameObject;
            // 이런 식으로..

            return result;
        }
        return null;
    }
}
