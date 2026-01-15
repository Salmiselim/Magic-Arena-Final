using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifetime = 5f;

    [Header("Combat")]
    private float damage;
    private float range;
    private Vector3 startPosition;
    private bool hasHit = false;

    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;

    void Start()
    {
        startPosition = transform.position;

        // Ensure we have proper components
        EnsureComponents();

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);

        Debug.Log($"🔮 Projectile spawned at {transform.position}, damage: {damage}, range: {range}");
    }

    void EnsureComponents()
    {
        // Make sure we have a Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = true; // We move it manually for better control
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Better for fast-moving objects

        // Make sure we have a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 0.2f;
            sphere.isTrigger = true;
            Debug.Log("✅ Added SphereCollider to projectile");
        }
        else
        {
            col.isTrigger = true;
            Debug.Log($"✅ Projectile has {col.GetType().Name}, set as trigger");
        }
    }

    public void Initialize(float damageAmount, float rangeAmount)
    {
        damage = damageAmount;
        range = rangeAmount;
        startPosition = transform.position;

        Debug.Log($"📊 Projectile initialized: Damage={damage}, Range={range}");
    }

    void Update()
    {
        if (hasHit) return;

        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Check if exceeded range
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled > range)
        {
            Debug.Log($"🎯 Projectile exceeded range ({distanceTraveled:F1}m / {range}m)");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return; // Prevent multiple hits

        Debug.Log($"🎯 Projectile trigger enter: {other.name} (Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        // Ignore player and other projectiles
        if (other.CompareTag("Player") || other.CompareTag("Projectile"))
        {
            Debug.Log("   ⏭️ Ignoring player/projectile");
            return;
        }

        // Check for BOTH EnemyFollow AND EnemyRanged
        EnemyFollow meleeEnemy = other.GetComponent<EnemyFollow>();
        EnemyRanged rangedEnemy = other.GetComponent<EnemyRanged>();

        bool hitEnemy = false;

        // Try melee enemy first
        if (meleeEnemy != null && meleeEnemy.IsAlive())
        {
            int finalDamage = Mathf.RoundToInt(damage);
            meleeEnemy.TakeDamage(finalDamage);
            Debug.Log($"💥 PROJECTILE HIT MELEE ENEMY! {other.name} took {finalDamage} damage");
            hitEnemy = true;

            // Knockback
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(transform.forward * 300f, ForceMode.Impulse);
            }
        }
        // Try ranged enemy
        else if (rangedEnemy != null && rangedEnemy.IsAlive())
        {
            int finalDamage = Mathf.RoundToInt(damage);
            rangedEnemy.TakeDamage(finalDamage);
            Debug.Log($"💥 PROJECTILE HIT RANGED ENEMY! {other.name} took {finalDamage} damage");
            hitEnemy = true;

            // Knockback
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(transform.forward * 300f, ForceMode.Impulse);
            }
        }

        // If we hit an enemy OR hit environment (wall, ground, etc.)
        if (hitEnemy || !other.CompareTag("Enemy"))
        {
            hasHit = true;

            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }

            // Play hit sound
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            // Destroy projectile
            Destroy(gameObject);
        }
    }

    // Backup collision detection (in case trigger doesn't work)
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🎯 Projectile COLLISION enter: {collision.gameObject.name}");

        // Treat collision same as trigger
        OnTriggerEnter(collision.collider);
    }
}