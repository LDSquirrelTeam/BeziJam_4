using UnityEngine;

public class WallEntity : MonoBehaviour
{
    [Header("Landing Settings")]
    public float health = 100f;
    public bool hasLanded = false;
    public float landingThreshold = -4f; // 기본 floor 위치 (자동 계산될 수도 있음)
    
    // 착지 시 호출될 이벤트
    public System.Action OnLanded;

    void Start()
    {
    }
    
    void Update()
    {
    }

    public void SetHealth()
    {
        health = 100f;
    }
    
    public void TriggerLanding()
    {
        if (hasLanded) return;

        hasLanded = true;
        OnLanded?.Invoke();
        
        Debug.Log($"Wall {gameObject.name} has landed!");
    }

    // 외부에서 착지 상태를 확인할 수 있는 메서드
    public bool IsLanded()
    {
        return hasLanded;
    }

    public void ResetToPoolState()
    {
        // Reset any wall-specific state here
        // For example: reset physics, animation states, etc.
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset any other state variables as needed
    }


}
