using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Vector2 damageSource);
    float GetCurrentHealth();
    bool IsAlive();
}