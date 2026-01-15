using UnityEngine;
using System.Collections;

/// <summary>
/// Handles elemental status effects on enemies (burn, freeze, stun, etc.)
/// Automatically attached to enemies when hit by elemental spells
/// </summary>
public class ElementalStatusEffect : MonoBehaviour
{
    private ElementType activeEffect = ElementType.Fire;
    private float effectEndTime;
    private float originalMoveSpeed;
    private bool isEffectActive = false;

    // For damage over time
    private float dotDamage;
    private float nextDotTime;
    private float dotInterval = 1f; // Damage every 1 second

    // For slow
    private float slowMultiplier;

    // Visual indicators
    private GameObject visualEffect;

    // References
    private EnemyFollow meleeEnemy;
    private EnemyRanged rangedEnemy;
    private Material originalMaterial;
    private Renderer enemyRenderer;

    void Start()
    {
        meleeEnemy = GetComponent<EnemyFollow>();
        rangedEnemy = GetComponent<EnemyRanged>();
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }
    }

    public void ApplyEffect(ElementType type, float duration, float dotDmg, float slowAmt)
    {
        Debug.Log($"🎯 Applying {type} effect to {gameObject.name} for {duration}s");

        // Remove previous effect
        if (isEffectActive)
        {
            RemoveEffect();
        }

        activeEffect = type;
        effectEndTime = Time.time + duration;
        isEffectActive = true;
        dotDamage = dotDmg;
        slowMultiplier = slowAmt;
        nextDotTime = Time.time + dotInterval;

        // Store original speed
        if (meleeEnemy != null)
        {
            originalMoveSpeed = meleeEnemy.moveSpeed;
        }
        else if (rangedEnemy != null)
        {
            originalMoveSpeed = rangedEnemy.moveSpeed;
        }

        // Apply type-specific effects
        switch (type)
        {
            case ElementType.Fire:
                ApplyBurn();
                break;
            case ElementType.Ice:
                ApplyFreeze();
                break;
            case ElementType.Lightning:
                ApplyStun();
                break;
            case ElementType.Earth:
                ApplyRoot();
                break;
            case ElementType.Wind:
                ApplyKnockback();
                break;
        }
    }

    void Update()
    {
        if (!isEffectActive) return;

        // Check if effect expired
        if (Time.time >= effectEndTime)
        {
            RemoveEffect();
            return;
        }

        // Apply damage over time (for fire)
        if (activeEffect == ElementType.Fire && Time.time >= nextDotTime)
        {
            ApplyDotDamage();
            nextDotTime = Time.time + dotInterval;
        }
    }

    void ApplyBurn()
    {
        Debug.Log($"🔥 {gameObject.name} is BURNING!");

        // Change color to red-ish
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
        }

        // Spawn fire particles (if you have a prefab)
        // GameObject fire = Instantiate(fireParticlePrefab, transform.position, Quaternion.identity, transform);
    }

    void ApplyFreeze()
    {
        Debug.Log($"❄️ {gameObject.name} is FROZEN!");

        // Change color to blue
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.cyan;
        }

        // Slow enemy movement
        if (meleeEnemy != null)
        {
            meleeEnemy.moveSpeed = originalMoveSpeed * slowMultiplier;
        }
        else if (rangedEnemy != null)
        {
            rangedEnemy.moveSpeed = originalMoveSpeed * slowMultiplier;
        }
    }

    void ApplyStun()
    {
        Debug.Log($"⚡ {gameObject.name} is STUNNED!");

        // Change color to yellow
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.yellow;
        }

        // Stop movement completely
        if (meleeEnemy != null)
        {
            meleeEnemy.moveSpeed = 0f;
        }
        else if (rangedEnemy != null)
        {
            rangedEnemy.moveSpeed = 0f;
        }
    }

    void ApplyRoot()
    {
        Debug.Log($"🌿 {gameObject.name} is ROOTED!");

        // Change color to green
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.green;
        }

        // Stop movement but can still attack
        if (meleeEnemy != null)
        {
            meleeEnemy.moveSpeed = 0f;
        }
        else if (rangedEnemy != null)
        {
            rangedEnemy.moveSpeed = 0f;
        }
    }

    void ApplyKnockback()
    {
        Debug.Log($"💨 {gameObject.name} is KNOCKED BACK!");

        // Wind just applies knockback, handled in spell script
        // No ongoing effect, so remove immediately
        Invoke(nameof(RemoveEffect), 0.5f);
    }

    void ApplyDotDamage()
    {
        int damage = Mathf.RoundToInt(dotDamage);

        if (meleeEnemy != null && meleeEnemy.IsAlive())
        {
            meleeEnemy.TakeDamage(damage);
            Debug.Log($"🔥 {gameObject.name} took {damage} burn damage!");
        }
        else if (rangedEnemy != null && rangedEnemy.IsAlive())
        {
            rangedEnemy.TakeDamage(damage);
            Debug.Log($"🔥 {gameObject.name} took {damage} burn damage!");
        }
    }

    void RemoveEffect()
    {
        if (!isEffectActive) return;

        Debug.Log($"✨ Removing {activeEffect} effect from {gameObject.name}");

        isEffectActive = false;

        // Restore original speed
        if (meleeEnemy != null)
        {
            meleeEnemy.moveSpeed = originalMoveSpeed;
        }
        else if (rangedEnemy != null)
        {
            rangedEnemy.moveSpeed = originalMoveSpeed;
        }

        // Restore original color
        if (enemyRenderer != null && originalMaterial != null)
        {
            enemyRenderer.material.color = originalMaterial.color;
        }

        // Destroy visual effects
        if (visualEffect != null)
        {
            Destroy(visualEffect);
        }

        // Remove component after a delay (in case multiple effects)
        Invoke(nameof(DestroySelf), 0.1f);
    }

    void DestroySelf()
    {
        if (!isEffectActive)
        {
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        // Make sure we restore speed if enemy dies while under effect
        if (meleeEnemy != null && originalMoveSpeed > 0)
        {
            meleeEnemy.moveSpeed = originalMoveSpeed;
        }
        else if (rangedEnemy != null && originalMoveSpeed > 0)
        {
            rangedEnemy.moveSpeed = originalMoveSpeed;
        }
    }
}