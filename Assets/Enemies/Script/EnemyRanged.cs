using UnityEngine;

/// <summary>
/// Ranged enemy that shoots projectiles
/// Alternative to EnemyFollow - attach this instead for archer-type enemies
/// </summary>
public class EnemyRanged : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private PlayerStats playerStats; // NEW: For giving drops
    private Animator anim;
    private EnemyHealthBar healthBar;
    private AudioSource audioSource;

    [Header("UI")]
    public GameObject healthBarPrefab;

    [Header("Combat")]
    public GameObject projectilePrefab;             // Projectile to shoot
    public Transform shootPoint;                    // Where projectile spawns (assign manually)
    public float shootRange = 15f;                  // How far they can shoot
    public float minShootDistance = 5f;             // Don't shoot if too close
    public float shootCooldown = 2f;                // Time between shots
    public float retreatDistance = 3f;              // Back up if player gets close

    [Header("Runtime Stats (Set by EnemyData)")]
    public float moveSpeed = 2f;
    public int maxHealth = 100;
    public int currentHealth;
    public int attackDamage = 10;

    private bool isActive = false;
    private float lastShootTime;

    [HideInInspector]
    public EnemyData enemyData;

    void Awake()
    {
        // Setup components FIRST (before Initialize is called)
        anim = GetComponent<Animator>();

        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f;

        // Auto-find shoot point if not assigned
        if (shootPoint == null)
        {
            // Create shoot point at chest height
            GameObject sp = new GameObject("ShootPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
            shootPoint = sp.transform;
        }

        Debug.Log($"✅ EnemyRanged Awake complete for {gameObject.name}");
    }

    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            // NEW: Get PlayerStats for giving drops
            playerStats = playerObj.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = playerObj.GetComponentInParent<PlayerStats>();
            }

            if (playerStats != null)
            {
                Debug.Log($"✅ EnemyRanged found PlayerStats");
            }
            else
            {
                Debug.LogWarning($"⚠️ EnemyRanged: Player has no PlayerStats!");
            }

            Debug.Log($"✅ EnemyRanged found player: {player.name}");
        }
        else
        {
            Debug.LogError("❌ EnemyRanged: No player found with 'Player' tag!");
        }
    }

    public void Initialize(EnemyData data)
    {
        Debug.Log($"🎯 EnemyRanged.Initialize START for {gameObject.name}");

        enemyData = data;
        moveSpeed = data.moveSpeed;
        maxHealth = data.maxHealth;
        currentHealth = data.maxHealth;
        attackDamage = data.attackDamage;

        Debug.Log($"   Stats set - Speed:{moveSpeed}, Health:{maxHealth}, Damage:{attackDamage}");

        if (data.skinMaterial != null)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.material = data.skinMaterial;
            }
            Debug.Log($"   Applied skin material");
        }

        // Spawn health bar AFTER initialization
        Debug.Log($"   Spawning health bar...");
        SpawnHealthBar();

        // Activate the enemy
        Debug.Log($"   Calling Activate()...");
        Activate();

        Debug.Log($"✅ EnemyRanged.Initialize COMPLETE - isActive={isActive}");
    }

    void SpawnHealthBar()
    {
        if (healthBarPrefab != null)
        {
            GameObject healthBarObj = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObj.GetComponent<EnemyHealthBar>();

            if (healthBar != null)
            {
                healthBar.SetTarget(transform);
                healthBar.UpdateHealth(currentHealth, maxHealth);
            }
        }

        // Play spawn sound
        if (enemyData?.spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enemyData.spawnSound);
        }
    }

    void Update()
    {
        if (!isActive) return;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Always face player
        FacePlayer();

        // Too close - retreat!
        if (distance < minShootDistance)
        {
            if (anim != null)
            {
                anim.SetBool("isWalking", true);
                anim.SetBool("isAttacking", false);
            }

            RetreatFromPlayer();
        }
        // In shooting range - shoot!
        else if (distance <= shootRange)
        {
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isAttacking", false);
            }

            // Shoot on cooldown
            if (Time.time >= lastShootTime + shootCooldown)
            {
                Shoot();
                lastShootTime = Time.time;
            }
        }
        // Too far - move closer
        else
        {
            if (anim != null)
            {
                anim.SetBool("isWalking", true);
                anim.SetBool("isAttacking", false);
            }

            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void RetreatFromPlayer()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        direction.y = 0;
        transform.position += direction * moveSpeed * 0.7f * Time.deltaTime;
    }

    void FacePlayer()
    {
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookPos),
                10f * Time.deltaTime
            );
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("⚠️ No projectile prefab assigned to ranged enemy!");
            return;
        }

        // Play attack animation trigger
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // Play attack sound
        if (enemyData != null && enemyData.attackSounds != null && enemyData.attackSounds.Length > 0)
        {
            AudioClip clip = enemyData.attackSounds[Random.Range(0, enemyData.attackSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        // Spawn projectile
        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 1.5f;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // NEW: Better aiming - aim at player's center, not feet
        Vector3 playerCenter = player.position + Vector3.up * 1f; // Aim at chest height
        Vector3 direction = (playerCenter - spawnPos).normalized;

        Debug.DrawRay(spawnPos, direction * 10f, Color.red, 2f); // Visualize the shot in Scene view

        EnemyProjectile projectile = proj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.damage = attackDamage;
            projectile.SetDirection(direction);
            Debug.Log($"🏹 Shot from {spawnPos} toward {playerCenter}, direction: {direction}");
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isActive) return;

        currentHealth -= damage;

        Debug.Log($"{(enemyData != null ? enemyData.enemyName : "Ranged Enemy")} took {damage} damage! HP: {currentHealth}/{maxHealth}");

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        // Play hit sound
        if (enemyData != null && enemyData.hitSounds != null && enemyData.hitSounds.Length > 0)
        {
            AudioClip clip = enemyData.hitSounds[Random.Range(0, enemyData.hitSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isActive = false;

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // Play death sound
        if (enemyData?.deathSound != null)
        {
            audioSource.PlayOneShot(enemyData.deathSound);
        }

        // Award rewards
        if (enemyData != null)
        {
            Debug.Log($"💀 Ranged enemy died! +{enemyData.scoreValue} score, +{enemyData.goldValue} gold");

            // NEW: Give health and mana drops to player
            if (playerStats != null)
            {
                if (enemyData.healthDrop > 0)
                {
                    playerStats.Heal(enemyData.healthDrop);
                    Debug.Log($"💚 +{enemyData.healthDrop} HP from {enemyData.enemyName}");
                }

                if (enemyData.manaDrop > 0)
                {
                    playerStats.ReplenishMana(enemyData.manaDrop);
                    Debug.Log($"💙 +{enemyData.manaDrop} Mana from {enemyData.enemyName}");
                }
            }
        }

        // Notify wave manager
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnEnemyDied(gameObject);
        }

        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        Destroy(gameObject, 10f);
    }

    public void Activate()
    {
        isActive = true;
        Debug.Log($"✅ EnemyRanged ACTIVATED: {gameObject.name}");
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}