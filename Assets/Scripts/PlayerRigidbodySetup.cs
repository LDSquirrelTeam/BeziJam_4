using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRigidbodySetup : MonoBehaviour
{
    [Header("Auto-Configure Rigidbody2D for Platformer")]
    [SerializeField] private bool autoConfigureOnStart = true;
    
    private void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureRigidbody2D();
        }
    }
    
    [ContextMenu("Configure Rigidbody2D")]
    public void ConfigureRigidbody2D()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // Core physics settings for 2D platformer
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3.5f;  // Faster falling for responsive feel
            rb.mass = 1f;
            rb.linearDamping = 0f;            // No linear drag for crisp movement
            rb.angularDamping = 0.05f;  // Minimal angular drag
            
            // Prevent rotation to avoid spinning
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // Smooth interpolation for visual quality
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            // Discrete collision detection (fastest)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            
            // Start awake for immediate response
            rb.sleepMode = RigidbodySleepMode2D.StartAwake;
            
            Debug.Log("Rigidbody2D configured for 2D platformer gameplay!");
        }
        else
        {
            Debug.LogError("No Rigidbody2D component found!");
        }
    }
}