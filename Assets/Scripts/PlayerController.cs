using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float airControlMultiplier = 0.8f;
    
    [Header("Jump Settings")]
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private bool resetJumpsOnGrounded = true;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    
    [Header("Animation")]
    [SerializeField] private bool flipSpriteX = true;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;
    
    private Vector2 moveInput;
    private bool jumpInput;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool facingRight = true;
    private int currentJumpCount = 0;
    
    private InputAction moveAction;
    private InputAction jumpAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
        }
    }

    private void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed += OnJump;
            jumpAction.canceled += OnJumpCanceled;
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.canceled -= OnJumpCanceled;
        }
    }

    private void Update()
    {
        HandleInput();
        CheckGroundStatus();
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleInput()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if(currentJumpCount >= maxJumpCount)
        {
            Debug.Log("Max jump count reached, cannot jump.");
            return;
        }
        Debug.Log("Jump input received");
        jumpInput = true;
        jumpBufferCounter = jumpBufferTime;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Jump input canceled");
        if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private void CheckGroundStatus()
    {
        wasGrounded = isGrounded;
        
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(transform.position + Vector3.down * 0.5f, groundCheckRadius, groundLayerMask);
        }
        Debug.Log($"Grounded: {isGrounded}, Was Grounded: {wasGrounded}, Current Jump Count: {currentJumpCount}");
        // Reset jump count when player lands on ground
        if (isGrounded && !wasGrounded && resetJumpsOnGrounded)
        {
            currentJumpCount = 0;
        }

        //if(isGrounded && resetJumpsOnGrounded)
        //{
        //    currentJumpCount = 0;

        //}
    }

    private void UpdateCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        float targetVelocityX = moveInput.x * moveSpeed;
        
        if (!isGrounded)
        {
            targetVelocityX *= airControlMultiplier;
        }
        
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
        
        if (moveInput.x != 0f)
        {
            facingRight = moveInput.x > 0f;
        }
    }

    private void HandleJump()
    {
        // Check if we can jump (either grounded with coyote time OR have remaining air jumps)
        bool canGroundJump = jumpBufferCounter > 0f && coyoteTimeCounter > 0f;
        bool canAirJump = !isGrounded && currentJumpCount < maxJumpCount && jumpBufferCounter > 0f;
        Debug.Log($"Jump Buffer: {jumpBufferCounter}, Coyote Time: {coyoteTimeCounter}, current Velocity {rb.linearVelocity}");
        Debug.Log($"Can Ground Jump: {canGroundJump}, Can Air Jump: {canAirJump}, Current Jump Count: {currentJumpCount}");

        //if (canGroundJump && !isGrounded)
        //{
        //    Debug.Log("ground smoouth jumping");
        //    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //    jumpBufferCounter = 0f;
        //    coyoteTimeCounter = 0f;
        //}
        //if (canAirJump)
        //{
        //    Debug.Log("air smoouth jumping");
        //    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //    jumpBufferCounter = 0f;
        //    currentJumpCount++;
        //}

        if (canGroundJump || canAirJump)
        {
            Debug.Log("Jumping!");
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;

            if (canGroundJump)
            {
                // First jump from ground
                currentJumpCount = 1;
                coyoteTimeCounter = 0f;
            }
            else if (canAirJump)
            {
                // Air jump
                currentJumpCount++;
            }
        }

        jumpInput = false;
    }

    //private void HandleJump()
    //{
    //    if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
    //    {
    //        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    //        jumpBufferCounter = 0f;
    //        coyoteTimeCounter = 0f;
    //    }

    //    jumpInput = false;
    //}


    private void UpdateVisuals()
    {
        if (spriteRenderer != null && flipSpriteX)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 groundCheckPos = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 0.5f;
        
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPos, groundCheckRadius);
    }
    
    // Public methods for debugging or external access
    public int GetCurrentJumpCount() => currentJumpCount;
    public int GetMaxJumpCount() => maxJumpCount;
    public bool CanJump() => (isGrounded && coyoteTimeCounter > 0f) || (!isGrounded && currentJumpCount < maxJumpCount);
}