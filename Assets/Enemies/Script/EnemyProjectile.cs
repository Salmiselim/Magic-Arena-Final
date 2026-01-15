using UnityEngine;

/// <summary>
/// Projectile fired by ranged enemies
/// FIXED: Now properly deals damage with knockback
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 10;
    public float speed = 15f;
    public float lifetime = 5f;
    public float knockbackForce = 5f;
    public bool destroyOnHit = true;
    public float collisionRadius = 0.5f; // NEW: Detection radius


    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip hitSound;
    public AudioClip flySound;

    [Header("Visual")]
    public TrailRenderer trail;

    private Vector3 direction;
    private AudioSource audioSource;
    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (flySound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = flySound;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.3f;
            audioSource.Play();
        }

        Debug.Log($"🏹 Enemy projectile spawned: Damage={damage}, Speed={speed}, Knockback={knockbackForce}");
    }

   void Update()
{
    // Move projectile
    Vector3 movement = direction * speed * Time.deltaTime;
    
    // Check for player along the path (better detection)
    RaycastHit hit;
    if (Physics.SphereCast(transform.position, collisionRadius, direction, out hit, movement.magnitude))
    {
        Debug.Log($"🎯 SphereCast hit: {hit.collider.name} (Tag: {hit.collider.tag})");
        
        // Check if it's the player
        PlayerStats ps = hit.collider.GetComponentInParent<PlayerStats>();
        if (ps != null || hit.collider.CompareTag("Player"))
        {
            DamagePlayer(hit.collider);
            return;
        }
    }
    
    
    transform.position += movement;
}

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        Debug.Log($"🎯 Projectile direction set to: {direction}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return; // ✅ Already here, good!

        Debug.Log($"🎯 Enemy projectile collided with: {other.name} (Tag: {other.tag})");

        // Try to find PlayerStats anywhere in the hierarchy
        PlayerStats playerCheck = other.GetComponentInParent<PlayerStats>();

        // If not found, search from root
        if (playerCheck == null)
        {
            Transform root = other.transform.root;
            playerCheck = root.GetComponentInChildren<PlayerStats>();
        }

        if (playerCheck != null)
        {
            Debug.Log($"💥 PLAYER HIT!");
            hasHit = true; // ⚠️ SET THIS IMMEDIATELY!
            DamagePlayer(other);
            return;
        }

        // HIT ENVIRONMENT
        if (!other.CompareTag("Enemy") && !other.CompareTag("Projectile"))
        {
            Debug.Log($"🌍 Hit environment: {other.name}");
            hasHit = true; // ⚠️ SET THIS HERE TOO!
            HitEffect(transform.position);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Damage the player - tries multiple methods to find PlayerStats
    /// </summary>
    void DamagePlayer(Collider playerCollider)
    {
        // ⚠️ Double-check we haven't already hit
        if (hasHit && destroyOnHit) return;
        hasHit = true;

        PlayerStats playerStats = null;

        // Method 1: On the collider itself
        playerStats = playerCollider.GetComponent<PlayerStats>();

        // Method 2: On parent
        if (playerStats == null)
        {
            playerStats = playerCollider.GetComponentInParent<PlayerStats>();
        }

        // Method 3: In children
        if (playerStats == null)
        {
            playerStats = playerCollider.GetComponentInChildren<PlayerStats>();
        }

        // Method 4: Find in scene (last resort)
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                Debug.LogWarning($"⚠️ Had to search entire scene for PlayerStats!");
            }
        }

        // APPLY DAMAGE - ONLY ONCE!
        if (playerStats != null)
        {
            Vector3 knockbackDir = direction;
            knockbackDir.y = 0;

            try
            {
                playerStats.TakeDamage(damage, knockbackDir, knockbackForce);
                Debug.Log($"💥 Dealt {damage} damage with {knockbackForce} knockback!");
            }
            catch
            {
                playerStats.TakeDamage(damage);
                Debug.Log($"💥 Dealt {damage} damage (no knockback method)");
            }

            Debug.Log($"   Player HP: {playerStats.currentHP}/{playerStats.maxHP}");
        }
        else
        {
            Debug.LogError($"❌ FAILED TO FIND PlayerStats!");
        }

        // Visual/audio feedback
        HitEffect(playerCollider.transform.position);

        // Destroy projectile IMMEDIATELY
        if (destroyOnHit)
        {
            Destroy(gameObject); // Changed from 0.1f delay to immediate
        }
    }
    void OnDrawGizmos()
    {
        // Draw the projectile's path
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * 2f);
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
    void HitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position, 0.5f);
        }
    }
}