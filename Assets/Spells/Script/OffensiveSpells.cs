using UnityEngine;

[CreateAssetMenu(fileName = "New Offensive Spell", menuName = "Spells/Offensive")]
public class OffensiveSpell : Spell
{
    [Header("Projectile Mode")]
    public float damage = 25f;
    public float range = 20f;
    public GameObject projectilePrefab;

    [Header("Ray Mode (if no projectile)")]
    public GameObject hitEffectPrefab;
    public LayerMask enemyLayer = ~0; // Hit everything by default

    public override void Cast(Vector3 origin, Vector3 direction)
    {
        Debug.Log($"🔮 Casting {spellName} from {origin} toward {direction}");

        // If spell uses a projectile → spawn projectile
        if (projectilePrefab != null)
        {
            SpawnProjectile(origin, direction);
            return;
        }

        // Otherwise use ray-based spell (instant hit)
        CastRay(origin, direction);
    }

    void SpawnProjectile(Vector3 origin, Vector3 direction)
    {
        // Spawn slightly forward to avoid hitting the caster
        Vector3 spawnPos = origin + direction.normalized * 0.5f;

        Debug.Log($"✨ Spawning projectile at {spawnPos}");

        GameObject proj = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction)
        );

        // Tag it so we can ignore player collisions
        proj.tag = "Projectile";

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.Initialize(damage, range);
            Debug.Log($"✅ Projectile initialized with damage={damage}, range={range}");
        }
        else
        {
            Debug.LogError("❌ Projectile prefab is missing Projectile script!");
        }
    }

    void CastRay(Vector3 origin, Vector3 direction)
    {
        Debug.Log($"⚡ Casting ray from {origin} in direction {direction}");

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range, enemyLayer))
        {
            Debug.Log($"🎯 Ray hit: {hit.collider.name}");

            // Check for both enemy types
            EnemyFollow meleeEnemy = hit.collider.GetComponent<EnemyFollow>();
            EnemyRanged rangedEnemy = hit.collider.GetComponent<EnemyRanged>();

            if (meleeEnemy != null && meleeEnemy.IsAlive())
            {
                int finalDamage = Mathf.RoundToInt(damage);
                meleeEnemy.TakeDamage(finalDamage);
                Debug.Log($"🔥 Ray spell HIT {meleeEnemy.name}! Damage: {finalDamage}");

                SpawnHitEffect(hit.point);
                ApplyKnockback(hit.collider, direction);
            }
            else if (rangedEnemy != null && rangedEnemy.IsAlive())
            {
                int finalDamage = Mathf.RoundToInt(damage);
                rangedEnemy.TakeDamage(finalDamage);
                Debug.Log($"🔥 Ray spell HIT {rangedEnemy.name}! Damage: {finalDamage}");

                SpawnHitEffect(hit.point);
                ApplyKnockback(hit.collider, direction);
            }
            else
            {
                Debug.Log($"⚠️ Hit object but no enemy component found: {hit.collider.name}");
            }
        }
        else
        {
            Debug.Log("❌ Ray missed - no hit");
        }
    }

    void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject fx = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(fx, 2f);
        }
    }

    void ApplyKnockback(Collider target, Vector3 direction)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(direction * 200f, ForceMode.Impulse);
        }
    }
}