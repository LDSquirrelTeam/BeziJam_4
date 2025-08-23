using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Damage Effects")]
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 0.5f;
    
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float damage, Vector2 damageSource)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);
        
        PlayDamageEffects();
        FlashRed();
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public bool IsAlive() => !isDead && currentHealth > 0f;

    private void PlayDamageEffects()
    {
        if (damageEffect != null)
            damageEffect.Play();
            
        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);
    }

    private void FlashRed()
    {
        if (spriteRenderer != null)
            StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died!");
        
        if (destroyOnDeath)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        Debug.Log($"{gameObject.name} healed for {healAmount}. Health: {currentHealth}/{maxHealth}");
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
}