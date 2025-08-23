using UnityEngine;

public class FallingWallController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float fallSpeed = 5f;
    public float startHeight = 10f;
    
    [Header("Reset Settings")]
    public bool autoReset = true;
    public float resetDelay = 2f;
    
    private Rigidbody2D rb;
    private Vector3 startPosition;
    private bool hasHitFloor = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = new Vector3(transform.position.x, startHeight, transform.position.z);
        ResetWall();
    }

    // case 1
    // script A
    // method A -> object 10k 

    // case 2
    // object 10k -> method B
    
    void Update()
    {
        // 벽이 화면 아래로 너무 많이 떨어지면 리셋
        if (transform.position.y < -8f && !hasHitFloor)
        {
            ResetWall();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "floor" && !hasHitFloor)
        {
            hasHitFloor = true;
            Debug.Log("Wall hit the floor!");
            
            // 벽이 바닥에 닿으면 속도를 0으로 만들어 멈추게 함
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            if (autoReset)
            {
                Invoke(nameof(ResetWall), resetDelay);
            }
        }
    }
    
    public void ResetWall()
    {
        hasHitFloor = false;
        transform.position = startPosition;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        
        // 중력 크기를 조정하여 떨어지는 속도 제어
        rb.gravityScale = fallSpeed / 9.81f;
    }
    
    public void TriggerFall()
    {
        if (!hasHitFloor)
        {
            ResetWall();
        }
    }
}