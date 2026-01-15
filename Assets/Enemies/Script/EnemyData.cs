using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identification")]
    public string enemyName = "Skeleton";
    public int level = 1;                           // 1, 2, or 3
    public EnemySpawner.Difficulty difficulty;      // Easy, Medium, Hard

    [Header("Visual")]
    public GameObject prefab;                       // The actual enemy prefab
    public Color portalColor = Color.green;         // Portal spawn color
    public Material skinMaterial;                   // Optional: different skin for this enemy

    [Header("Audio")]
    public AudioClip spawnSound;                    // Sound when spawning
    public AudioClip[] walkSounds;                  // Footstep sounds (random)
    public AudioClip[] hitSounds;                   // Pain sounds when hit (random)
    public AudioClip[] attackSounds;                // Attack grunt sounds (random)
    public AudioClip deathSound;                    // Death scream
    public float walkSoundInterval = 0.5f;          // Time between footsteps

    [Header("Stats")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    public float knockbackForce = 0f;               // NEW: 0 = no knockback, 5-15 = knockback

    [Header("Rewards")]
    public int scoreValue = 10;                     // Points when killed
    public int goldValue = 5;                       // Currency when killed
    public float healthDrop = 0f;                   // NEW: Health restored on kill (0 = none)
    public float manaDrop = 0f;                     // NEW: Mana restored on kill (0 = none)

    [Header("Spawn Settings")]
    public int spawnCount = 3;                      // How many spawn in this wave
    public float spawnDelay = 2f;                   // Delay between spawns in wave
}