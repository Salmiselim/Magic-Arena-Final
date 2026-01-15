using UnityEngine;
using System.Collections;

public enum ElementType { Fire, Ice, Lightning, Earth, Wind }

[CreateAssetMenu(fileName = "New Elemental Spell", menuName = "Spells/Elemental")]
public class ElementalMagic : Spell
{
    [Header("Elemental Settings")]
    public ElementType type;
    public float damage = 30f;
    public float range = 15f;
    public float areaRadius = 3f; // Area of effect

    [Header("Visual Effects")]
    public GameObject effectPrefab; // Particle effect for wand
    public GameObject impactEffectPrefab; // Effect at hit location
    public GameObject auraEffectPrefab; // Area effect visual

    [Header("Status Effects")]
    public float statusDuration = 3f; // How long burn/freeze/stun lasts
    public float dotDamage = 5f; // Damage over time (for fire)
    public float slowAmount = 0.5f; // Speed multiplier (for ice)

    [Header("Advanced")]
    public bool isAreaEffect = false; // True for AOE spells
    public LayerMask enemyLayer; // Which layers to hit

    public override void Cast(Vector3 origin, Vector3 direction)
    {
        Debug.Log($"🔮 Casting {spellName} ({type}) from {origin} toward {direction}");

        // Spawn wand effect
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, origin, Quaternion.LookRotation(direction));
            Destroy(effect, 2f);
        }

        // Perform raycast
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            Debug.Log($"✨ {spellName} hit {hit.collider.name} at {hit.point}");

            // Spawn impact effect
            if (impactEffectPrefab != null)
            {
                GameObject impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }

            if (isAreaEffect)
            {
                // Area of Effect damage
                ApplyAreaEffect(hit.point);
            }
            else
            {
                // Single target damage
                ApplySingleTargetEffect(hit.collider.gameObject, direction);
            }
        }
        else
        {
            Debug.Log($"⚠️ {spellName} missed (no hit within {range}m)");
        }
    }

    void ApplySingleTargetEffect(GameObject target, Vector3 direction)
    {
        // Try melee enemy first
        EnemyFollow meleeEnemy = target.GetComponent<EnemyFollow>();
        if (meleeEnemy != null && meleeEnemy.IsAlive())
        {
            ApplyElementalEffect(meleeEnemy.gameObject, direction);
            return;
        }

        // Try ranged enemy
        EnemyRanged rangedEnemy = target.GetComponent<EnemyRanged>();
        if (rangedEnemy != null && rangedEnemy.IsAlive())
        {
            ApplyElementalEffect(rangedEnemy.gameObject, direction);
            return;
        }

        Debug.Log($"⚠️ Hit {target.name} but no enemy component found");
    }

    void ApplyAreaEffect(Vector3 center)
    {
        Debug.Log($"💥 {type} AOE at {center}, radius: {areaRadius}");

        // Spawn area visual
        if (auraEffectPrefab != null)
        {
            GameObject aura = Instantiate(auraEffectPrefab, center, Quaternion.identity);
            aura.transform.localScale = Vector3.one * areaRadius * 2f;
            Destroy(aura, 2f);
        }

        // Find all enemies in radius
        Collider[] hits = Physics.OverlapSphere(center, areaRadius);
        int enemiesHit = 0;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 direction = (hit.transform.position - center).normalized;
                ApplyElementalEffect(hit.gameObject, direction);
                enemiesHit++;
            }
        }

        Debug.Log($"💥 {type} AOE hit {enemiesHit} enemies");
    }

    void ApplyElementalEffect(GameObject enemy, Vector3 direction)
    {
        // Get enemy component
        EnemyFollow meleeEnemy = enemy.GetComponent<EnemyFollow>();
        EnemyRanged rangedEnemy = enemy.GetComponent<EnemyRanged>();

        if (meleeEnemy == null && rangedEnemy == null)
        {
            Debug.LogWarning($"⚠️ {enemy.name} has no enemy script!");
            return;
        }

        int finalDamage = Mathf.RoundToInt(damage);

        // Apply immediate damage
        if (meleeEnemy != null)
        {
            meleeEnemy.TakeDamage(finalDamage);
        }
        else if (rangedEnemy != null)
        {
            rangedEnemy.TakeDamage(finalDamage);
        }

        Debug.Log($"🔥 {type} spell hit {enemy.name} for {finalDamage} damage!");

        // Apply elemental status effect
        ElementalStatusEffect status = enemy.GetComponent<ElementalStatusEffect>();
        if (status == null)
        {
            status = enemy.AddComponent<ElementalStatusEffect>();
        }

        status.ApplyEffect(type, statusDuration, dotDamage, slowAmount);

        // Apply knockback force
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float knockbackForce = type == ElementType.Lightning ? 20f : 10f;
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }

        // Type-specific visual feedback
        SpawnStatusVisual(enemy.transform);
    }

    void SpawnStatusVisual(Transform target)
    {
        // You can add particle systems for burn, freeze, etc.
        // For now, just log it
        Debug.Log($"✨ {type} visual effect on {target.name}");
    }
}