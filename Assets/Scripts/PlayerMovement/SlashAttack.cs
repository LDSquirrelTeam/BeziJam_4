using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlashAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 90f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask damageableLayers = -1;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem slashParticles;
    [SerializeField] private Transform slashEffectPoint;
    [SerializeField] private bool createDefaultParticles = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip slashSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;
    
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private SpriteRenderer spriteRenderer;
    private List<IDamageable> hitTargets = new List<IDamageable>();
    
    public bool IsAttacking => isAttacking;
    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        SetupDefaultParticles();
        SetupSlashEffectPoint();
    }

    private void SetupDefaultParticles()
    {
        if (slashParticles == null && createDefaultParticles)
        {
            GameObject particleGO = new GameObject("SlashParticles");
            particleGO.transform.SetParent(transform);
            particleGO.transform.localPosition = Vector3.zero;
            
            slashParticles = particleGO.AddComponent<ParticleSystem>();
            var main = slashParticles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.3f;
            main.startColor = Color.white;
            main.maxParticles = 20;
            
            var emission = slashParticles.emission;
            emission.enabled = false;
            
            var shape = slashParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = attackAngle * 0.5f;
            shape.radius = 0.1f;
        }
    }

    private void SetupSlashEffectPoint()
    {
        if (slashEffectPoint == null)
        {
            GameObject effectPoint = new GameObject("SlashEffectPoint");
            effectPoint.transform.SetParent(transform);
            effectPoint.transform.localPosition = new Vector3(attackRange * 0.5f, 0f, 0f);
            slashEffectPoint = effectPoint.transform;
        }
    }

    public void PerformSlash()
    {
        if (!CanAttack)
            return;
            
        StartCoroutine(SlashCoroutine());
    }

    private IEnumerator SlashCoroutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        hitTargets.Clear();
        
        PlaySlashEffects();
        DetectAndDamageTargets();
        
        yield return new WaitForSeconds(attackDuration);
        
        isAttacking = false;
    }

    private void PlaySlashEffects()
    {
        if (slashParticles != null)
        {
            UpdateParticleDirection();
            slashParticles.Emit(15);
        }
        
        if (audioSource != null && slashSound != null)
        {
            audioSource.PlayOneShot(slashSound);
        }
    }

    private void UpdateParticleDirection()
    {
        if (slashParticles == null) return;
        
        bool facingRight = spriteRenderer == null || !spriteRenderer.flipX;
        
        var shape = slashParticles.shape;
        if (facingRight)
        {
            shape.rotation = new Vector3(0, 0, 0);
            slashEffectPoint.localPosition = new Vector3(attackRange * 0.5f, 0f, 0f);
        }
        else
        {
            shape.rotation = new Vector3(0, 0, 180f);
            slashEffectPoint.localPosition = new Vector3(-attackRange * 0.5f, 0f, 0f);
        }
    }

    private void DetectAndDamageTargets()
    {
        Vector2 attackCenter = slashEffectPoint.position;
        bool facingRight = spriteRenderer == null || !spriteRenderer.flipX;
        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackCenter, attackRange, damageableLayers);
        
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.gameObject == gameObject) continue;
            
            if (IsTargetInAttackAngle(collider.transform.position, attackCenter, facingRight))
            {
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null && !hitTargets.Contains(damageable))
                {
                    damageable.TakeDamage(damage, attackCenter);
                    hitTargets.Add(damageable);
                }
                
                Rigidbody2D targetRb = collider.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    Vector2 knockbackDirection = (collider.transform.position - transform.position).normalized;
                    targetRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
                }
            }
        }
    }

    private bool IsTargetInAttackAngle(Vector3 targetPos, Vector3 attackCenter, bool facingRight)
    {
        Vector2 directionToTarget = (targetPos - attackCenter).normalized;
        Vector2 attackDirection = facingRight ? Vector2.right : Vector2.left;
        
        float angleToTarget = Vector2.Angle(attackDirection, directionToTarget);
        return angleToTarget <= attackAngle * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Vector3 center = slashEffectPoint != null ? slashEffectPoint.position : transform.position;
        bool facingRight = spriteRenderer == null || !spriteRenderer.flipX;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(center, attackRange);
        
        Vector3 attackDirection = facingRight ? Vector3.right : Vector3.left;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, attackAngle * 0.5f) * attackDirection;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -attackAngle * 0.5f) * attackDirection;
        
        Gizmos.DrawRay(center, leftBoundary * attackRange);
        Gizmos.DrawRay(center, rightBoundary * attackRange);
        Gizmos.DrawRay(center, attackDirection * attackRange);
    }
}