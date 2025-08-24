using UnityEngine;
using UnityEngine.InputSystem;

public class SlashAttackSecond : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float maxAttackDistance = 5f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Particle Effect")]
    [SerializeField] private GameObject slashParticleParent;
    [SerializeField] private ParticleSystem slashParticles;
    [SerializeField] private ParticleSystem slashBackParticles;
    [SerializeField] private bool createParticleIfNull = true;
    [SerializeField] private bool rotateTowardsTarget = true;
    [SerializeField] private float rotationOffset = 0f;

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool autoFindPlayer = true;

    private Camera mainCamera;
    private PlayerInput playerInput;
    private InputAction attackAction;
    private float lastAttackTime;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

        if (autoFindPlayer && playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                playerTransform = transform;
                Debug.LogWarning("Player not found, using this transform as player reference");
            }
        }

        if (createParticleIfNull && slashParticles == null)
        {
            CreateDefaultParticleSystem();
        }

        if (playerInput != null)
        {
            attackAction = playerInput.actions["Attack"];
            if (attackAction == null)
            {
                Debug.LogWarning("Attack action not found in Input System. Make sure to add 'Attack' action or use mouse click detection.");
            }
        }
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.performed += OnAttackPerformed;
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
        }
    }

    private void Update()
    {
        if (attackAction == null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformSlashAttack();
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        PerformSlashAttack();
    }

    private void PerformSlashAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 attackPosition = GetLimitedAttackPosition(mouseWorldPosition);

        PlaySlashParticles(attackPosition);
        lastAttackTime = Time.time;
    }

    private Vector3 GetMouseWorldPosition()
    {
        // give me editor not in play mode condition
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            // 에디터에서 플레이 모드가 아닐 때는 마우스 위치를 Scene 뷰 기준으로 반환하거나 Vector3.zero 반환
            return Vector3.zero;
        }
#endif



        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found");
            return Vector3.zero;
        }

        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    private Vector3 GetLimitedAttackPosition(Vector3 targetPosition)
    {
        if (playerTransform == null)
        {
            return targetPosition;
        }

        Vector3 playerPosition = playerTransform.position;
        Vector3 direction = (targetPosition - playerPosition).normalized;
        float distance = Vector3.Distance(playerPosition, targetPosition);

        if (distance > maxAttackDistance)
        {
            return playerPosition + direction * maxAttackDistance;
        }

        return targetPosition;
    }

    private void PlaySlashParticles(Vector3 position)
    {
        if (slashParticles == null)
        {
            Debug.LogWarning("No particle system assigned for slash attack");
            return;
        }

        slashParticleParent.transform.position = position;

        if (rotateTowardsTarget && playerTransform != null)
        {
            RotateParticleTowardsTarget(position);
        }

        slashParticles.Play();
        slashBackParticles?.Play();
    }

    private void RotateParticleTowardsTarget(Vector3 targetPosition)
    {
        Vector3 playerPosition = playerTransform.position;
        Vector3 direction = (targetPosition - playerPosition).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotationOffset;

        slashParticleParent.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void CreateDefaultParticleSystem()
    {
        GameObject particleObject = new GameObject("SlashParticles");
        particleObject.transform.SetParent(transform);

        slashParticles = particleObject.AddComponent<ParticleSystem>();

        var main = slashParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startColor = Color.white;
        main.maxParticles = 50;

        var emission = slashParticles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 20)
        });

        var shape = slashParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.1f;
        shape.length = 1f;

        var velocityOverLifetime = slashParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f, 10f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f, 2f);

        Debug.Log("Created default particle system for slash attack");
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerTransform.position, maxAttackDistance);

        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 limitedPos = GetLimitedAttackPosition(mousePos);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(limitedPos, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerTransform.position, limitedPos);

        if (rotateTowardsTarget)
        {
            Vector3 direction = (limitedPos - playerTransform.position).normalized;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(limitedPos, direction * 0.5f);
        }
    }
}
