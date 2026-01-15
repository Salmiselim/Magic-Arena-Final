using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner spawner;
    public Transform player;
    public SpawnPointManager spawnPointManager; // NEW: Reference to spawn point manager

    [Header("Level Enemies - MUST BE SIZE 3")]
    public EnemyData easyEnemy;     // Assign Level1_Easy here
    public EnemyData mediumEnemy;   // Assign Level1_Medium here
    public EnemyData hardEnemy;     // Assign Level1_Hard here

    [Header("Wave Settings")]
    public bool useFixedSpawnPoints = true;  // NEW: Use fixed spawn points or random around player
    public float spawnRadius = 15f;
    public float minSpawnDistance = 8f;
    public float timeBetweenWaves = 5f;

    [Header("Portal Settings")]
    public GameObject portalPrefab;          // Assign your portal prefab here
    public float portalSpawnDistance = 10f;  // Distance from player to spawn portal
    private GameObject spawnedPortal;

    [Header("Debug Info - READ ONLY")]
    public int currentWave = 0;         // 0=Easy, 1=Medium, 2=Hard
    public int totalEnemiesInWave = 0;
    public int aliveEnemies = 0;
    public bool waveActive = false;
    public bool allWavesComplete = false;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        Debug.Log("=== WAVE MANAGER START ===");

        // Auto-find references
        if (spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawner>();
            Debug.Log("Auto-found EnemySpawner: " + (spawner != null));
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Auto-found Player: " + player.name);
            }
        }

        // NEW: Auto-find spawn point manager
        if (spawnPointManager == null)
        {
            spawnPointManager = FindObjectOfType<SpawnPointManager>();
            if (spawnPointManager != null)
            {
                Debug.Log("✅ Auto-found SpawnPointManager");
            }
            else if (useFixedSpawnPoints)
            {
                Debug.LogWarning("⚠️ useFixedSpawnPoints is TRUE but no SpawnPointManager found!");
            }
        }

        // Validate
        if (easyEnemy == null || mediumEnemy == null || hardEnemy == null)
        {
            Debug.LogError("❌ WaveManager: You must assign all 3 enemy types in Inspector!");
            return;
        }

        if (spawner == null)
        {
            Debug.LogError("❌ WaveManager: No EnemySpawner found!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("❌ WaveManager: No Player found! Make sure player has 'Player' tag.");
            return;
        }

        if (portalPrefab == null)
        {
            Debug.LogWarning("⚠️ WaveManager: No portal prefab assigned! Portal won't spawn after waves.");
        }

        Debug.Log("✅ All references valid. Starting first wave in 3 seconds...");

        // Start first wave
        Invoke("StartNextWave", 3f);
    }

    void StartNextWave()
    {
        if (currentWave >= 3)
        {
            Debug.Log("🎉 ALL WAVES COMPLETE!");
            return;
        }

        waveActive = true;

        // Get current wave data
        EnemyData data = GetCurrentWaveData();

        Debug.Log($"🌊 WAVE {currentWave + 1} START: {data.enemyName}");
        Debug.Log($"   Spawning {data.spawnCount} enemies");

        totalEnemiesInWave = data.spawnCount;
        aliveEnemies = 0;

        // Spawn the wave
        StartCoroutine(SpawnWaveCoroutine(data));
    }

    EnemyData GetCurrentWaveData()
    {
        switch (currentWave)
        {
            case 0: return easyEnemy;
            case 1: return mediumEnemy;
            case 2: return hardEnemy;
            default: return easyEnemy;
        }
    }

    IEnumerator SpawnWaveCoroutine(EnemyData data)
    {
        for (int i = 0; i < data.spawnCount; i++)
        {
            // Get random spawn position
            Vector3 spawnPos = GetRandomSpawnPosition();

            Debug.Log($"Spawning enemy {i + 1}/{data.spawnCount} at {spawnPos}");

            yield return StartCoroutine(SpawnSingleEnemy(spawnPos, data));

            // Wait before next spawn
            yield return new WaitForSeconds(data.spawnDelay);
        }

        Debug.Log($"All {data.spawnCount} enemies spawned for this wave!");
    }

    IEnumerator SpawnSingleEnemy(Vector3 position, EnemyData data)
    {
        Debug.Log($"🔵 SpawnSingleEnemy START for {data.enemyName} at {position}");

        GameObject spawnedEnemy = null;
        bool callbackCalled = false;

        // Spawn with portal animation
        yield return StartCoroutine(spawner.SpawnSequenceWithData(
            position,
            data,
            (enemyObj, enemyData) => {
                spawnedEnemy = enemyObj;
                callbackCalled = true;
                Debug.Log($"🟢 Spawn callback received! Enemy: {(enemyObj != null ? enemyObj.name : "NULL")}");
            }
        ));

        Debug.Log($"🟡 After SpawnSequenceWithData - Callback called: {callbackCalled}, Enemy: {(spawnedEnemy != null ? spawnedEnemy.name : "NULL")}");

        if (spawnedEnemy != null)
        {
            // Make sure it has Enemy tag
            if (spawnedEnemy.tag != "Enemy")
            {
                Debug.LogWarning($"Enemy spawned without 'Enemy' tag! Setting it now.");
                spawnedEnemy.tag = "Enemy";
            }

            // Try to initialize - check for BOTH EnemyFollow AND EnemyRanged
            EnemyFollow meleeEnemy = spawnedEnemy.GetComponent<EnemyFollow>();
            EnemyRanged rangedEnemy = spawnedEnemy.GetComponent<EnemyRanged>();

            Debug.Log($"🔍 Components check - Melee: {meleeEnemy != null}, Ranged: {rangedEnemy != null}");

            bool initialized = false;

            if (meleeEnemy != null)
            {
                Debug.Log($"⚔️ Initializing as MELEE enemy...");
                meleeEnemy.Initialize(data);
                initialized = true;
                Debug.Log($"✅ Melee enemy initialized: {spawnedEnemy.name}");
            }
            else if (rangedEnemy != null)
            {
                Debug.Log($"🏹 Initializing as RANGED enemy...");
                rangedEnemy.Initialize(data);
                initialized = true;
                Debug.Log($"✅ Ranged enemy initialized: {spawnedEnemy.name}");
            }
            else
            {
                Debug.LogError($"❌ Enemy has NEITHER EnemyFollow NOR EnemyRanged script! GameObject: {spawnedEnemy.name}");
            }

            if (initialized)
            {
                // Track it
                spawnedEnemies.Add(spawnedEnemy);
                aliveEnemies++;

                Debug.Log($"✅ Enemy spawned and tracked! Name: {spawnedEnemy.name}, Tag: {spawnedEnemy.tag}, Alive: {aliveEnemies}/{totalEnemiesInWave}");
            }
        }
        else
        {
            Debug.LogError("❌ Enemy failed to spawn - spawnedEnemy is NULL!");
        }

        Debug.Log($"🔵 SpawnSingleEnemy END");
    }

    Vector3 GetRandomSpawnPosition()
    {
        // NEW: Check if we should use fixed spawn points
        if (useFixedSpawnPoints && spawnPointManager != null)
        {
            Vector3 fixedPos = spawnPointManager.GetSpawnPosition();
            Debug.Log($"✅ Using FIXED spawn point at {fixedPos}");
            return fixedPos;
        }

        // FALLBACK: Old random spawning around player
        Debug.Log("⚠️ Using OLD random spawn (no spawn points available)");

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, spawnRadius);

        Vector3 spawnPos = player.position + new Vector3(
            Mathf.Cos(angle) * distance,
            0,
            Mathf.Sin(angle) * distance
        );

        spawnPos.y = 0;
        return spawnPos;
    }
    public void OnEnemyDied(GameObject enemyObject)
    {
        Debug.Log($"📢 WaveManager.OnEnemyDied called for {(enemyObject != null ? enemyObject.name : "NULL")}");

        // Remove from tracking
        if (enemyObject != null)
        {
            spawnedEnemies.Remove(enemyObject);
            Debug.Log($"  ✅ Removed from tracking list");
        }

        aliveEnemies--;

        Debug.Log($"  📊 Remaining alive: {aliveEnemies}/{totalEnemiesInWave}");

        // Check if wave complete
        if (aliveEnemies <= 0 && waveActive)
        {
            Debug.Log($"  🎉 Wave complete condition met!");
            WaveComplete();
        }
    }

    public void OnEnemyDied(EnemyFollow enemy)
    {
        if (enemy != null)
        {
            OnEnemyDied(enemy.gameObject);
        }
    }

    void WaveComplete()
    {
        waveActive = false;
        currentWave++;

        Debug.Log($"✅✅✅ WAVE {currentWave} COMPLETE! ✅✅✅");

        if (currentWave >= 3)
        {
            // ALL WAVES COMPLETE - SPAWN PORTAL!
            Debug.Log("🎉🎉🎉 ALL WAVES COMPLETE! 🎉🎉🎉");
            allWavesComplete = true;
            SpawnPortal();
        }
        else
        {
            Debug.Log($"Next wave starts in {timeBetweenWaves} seconds...");
            Invoke("StartNextWave", timeBetweenWaves);
        }
    }

    void SpawnPortal()
    {
        if (portalPrefab == null)
        {
            Debug.LogError("❌ Cannot spawn portal - portalPrefab is not assigned!");
            return;
        }

        if (spawnedPortal != null)
        {
            Debug.LogWarning("⚠️ Portal already spawned!");
            return;
        }

        // Calculate spawn position in front of player
        Vector3 portalPosition = player.position + player.forward * portalSpawnDistance;
        portalPosition.y = 0; // Keep on ground

        Debug.Log($"🌀 Spawning portal at {portalPosition}");

        // Spawn the portal
        spawnedPortal = Instantiate(portalPrefab, portalPosition, Quaternion.identity);

        // Try to activate portal animation
        LevelPortal portal = spawnedPortal.GetComponent<LevelPortal>();
        if (portal != null)
        {
            portal.Activate(); // Plays spawn animation
            Debug.Log("✅ Portal spawned with animation!");
        }
        else
        {
            Debug.LogWarning("⚠️ Portal spawned but has no LevelPortal script!");
        }
    }

    void Update()
    {
        // Only use New Input System
        if (Keyboard.current != null)
        {
            // Press N to kill all enemies
            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                Debug.Log("⏭️ N PRESSED - KILLING ALL ENEMIES");
                NuclearKillAll();
            }

            // Press H to damage all enemies (50 damage)
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                Debug.Log("💥 H PRESSED - DAMAGING ALL ENEMIES");
                DamageAllEnemies(50);
            }

            // Press J to damage all enemies (10 damage)
            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                Debug.Log("💥 J PRESSED - SMALL DAMAGE TO ALL ENEMIES");
                DamageAllEnemies(10);
            }

            // Press P to spawn portal manually (for testing)
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("🌀 P PRESSED - MANUALLY SPAWNING PORTAL");
                SpawnPortal();
            }

            // Press I for debug info
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                Debug.Log("=== DEBUG INFO ===");
                Debug.Log($"Current Wave: {currentWave + 1}/3");
                Debug.Log($"Wave Active: {waveActive}");
                Debug.Log($"All Waves Complete: {allWavesComplete}");
                Debug.Log($"Alive Enemies: {aliveEnemies}/{totalEnemiesInWave}");
                Debug.Log($"Spawned List Count: {spawnedEnemies.Count}");
                Debug.Log($"Portal Spawned: {spawnedPortal != null}");
                Debug.Log($"Easy Enemy: {(easyEnemy != null ? easyEnemy.name : "NULL")}");
                Debug.Log($"Medium Enemy: {(mediumEnemy != null ? mediumEnemy.name : "NULL")}");
                Debug.Log($"Hard Enemy: {(hardEnemy != null ? hardEnemy.name : "NULL")}");

                // List all enemies
                Debug.Log("=== SPAWNED ENEMIES ===");
                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    if (spawnedEnemies[i] != null)
                    {
                        Debug.Log($"  {i}: {spawnedEnemies[i].name} (Active: {spawnedEnemies[i].activeSelf})");
                    }
                    else
                    {
                        Debug.Log($"  {i}: NULL");
                    }
                }
            }
        }
    }

    void DamageAllEnemies(int damageAmount)
    {
        Debug.Log($"💥💥💥 DAMAGING ALL ENEMIES ({damageAmount} damage) 💥💥💥");

        EnemyFollow[] meleeEnemies = FindObjectsOfType<EnemyFollow>();
        EnemyRanged[] rangedEnemies = FindObjectsOfType<EnemyRanged>();

        int totalEnemies = meleeEnemies.Length + rangedEnemies.Length;
        Debug.Log($"Found {totalEnemies} enemies ({meleeEnemies.Length} melee, {rangedEnemies.Length} ranged)");

        int damagedCount = 0;

        foreach (EnemyFollow enemy in meleeEnemies)
        {
            if (enemy != null && enemy.IsAlive())
            {
                Debug.Log($"  💥 Damaging melee: {enemy.gameObject.name}");
                enemy.TakeDamage(damageAmount);
                damagedCount++;
            }
        }

        foreach (EnemyRanged enemy in rangedEnemies)
        {
            if (enemy != null && enemy.IsAlive())
            {
                Debug.Log($"  💥 Damaging ranged: {enemy.gameObject.name}");
                enemy.TakeDamage(damageAmount);
                damagedCount++;
            }
        }

        Debug.Log($"💥 Damaged {damagedCount} enemies for {damageAmount} damage each!");
    }

    void NuclearKillAll()
    {
        Debug.Log("🔥🔥🔥 NUCLEAR KILL ACTIVATED 🔥🔥🔥");

        StopAllCoroutines();

        EnemyFollow[] meleeEnemies = FindObjectsOfType<EnemyFollow>();
        EnemyRanged[] rangedEnemies = FindObjectsOfType<EnemyRanged>();

        int totalEnemies = meleeEnemies.Length + rangedEnemies.Length;
        Debug.Log($"Found {totalEnemies} enemies ({meleeEnemies.Length} melee, {rangedEnemies.Length} ranged)");

        int killedCount = 0;

        foreach (EnemyFollow enemy in meleeEnemies)
        {
            if (enemy != null)
            {
                bool wasAlive = enemy.IsAlive();
                Debug.Log($"  💀 Melee: {enemy.gameObject.name}, Alive: {wasAlive}");
                enemy.TakeDamage(9999);
                killedCount++;
            }
        }

        foreach (EnemyRanged enemy in rangedEnemies)
        {
            if (enemy != null)
            {
                bool wasAlive = enemy.IsAlive();
                Debug.Log($"  💀 Ranged: {enemy.gameObject.name}, Alive: {wasAlive}, Active: {enemy.isActiveAndEnabled}");
                enemy.TakeDamage(9999);
                killedCount++;
            }
        }

        Debug.Log($"💀 Killed {killedCount} enemies");

        spawnedEnemies.Clear();
        aliveEnemies = 0;

        if (waveActive)
        {
            Debug.Log("🔥 Force completing wave");
            CancelInvoke();
            WaveComplete();
        }

        Debug.Log("🔥 Nuclear kill complete!");
    }
}