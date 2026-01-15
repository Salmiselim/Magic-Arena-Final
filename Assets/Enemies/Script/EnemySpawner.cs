using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject spawnPortalPrefab;

    [Header("Difficulty Colors")]
    public Color easyColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color hardColor = Color.red;

    [Header("Portal Animation")]
    public float portalOpenTime = 1f;
    public float portalHoldTime = 0.5f;
    public float portalCloseTime = 0.8f;
    public float maxPortalScale = 3f;
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Enemy Spawn Animation")]
    public float enemyRiseHeight = 2f;
    public float enemyRiseTime = 1.2f;

    [Header("Spawn Settings")]
    public float spawnHeightOffset = 0.1f;
    public float navMeshSearchRadius = 6f;

    [Header("Auto Spawn Testing")]
    public bool autoSpawnForTesting = false;
    public float autoSpawnInterval = 5f;
    private float nextAutoSpawnTime;

    public enum Difficulty { Easy, Medium, Hard }

    // =========================================================
    // PUBLIC API
    // =========================================================

    public void SpawnEnemy(Vector3 position, Difficulty difficulty)
    {
        Vector3 spawnPos = GetSpawnPosition(position);
        StartCoroutine(SpawnSequenceInternal(
            spawnPos,
            difficulty,
            enemyPrefab,
            GetDifficultyColor(difficulty),
            null
        ));
    }

    public IEnumerator SpawnSequenceWithData(
        Vector3 position,
        EnemyData data,
        System.Action<GameObject, EnemyData> onComplete)
    {
        Vector3 spawnPos = GetSpawnPosition(position);
        GameObject spawnedEnemy = null;

        yield return StartCoroutine(SpawnSequenceInternal(
            spawnPos,
            data.difficulty,
            data.prefab,
            data.portalColor,
            (enemy) =>
            {
                spawnedEnemy = enemy;
            }
        ));

        if (spawnedEnemy != null)
        {
            onComplete?.Invoke(spawnedEnemy, data);
        }
        else
        {
            Debug.LogError("EnemySpawner: Failed to spawn enemy with data.");
        }
    }

    // =========================================================
    // SPAWN POSITION (NAVMESH FIRST, RAYCAST FALLBACK)
    // =========================================================

    private Vector3 GetSpawnPosition(Vector3 intendedPos)
    {
        // PRIMARY: NavMesh
        if (NavMesh.SamplePosition(intendedPos, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            return hit.position + Vector3.up * spawnHeightOffset;
        }

        // FALLBACK: Physics
        if (Physics.Raycast(intendedPos + Vector3.up * 100f, Vector3.down, out RaycastHit rayHit, 200f))
        {
            return rayHit.point + Vector3.up * spawnHeightOffset;
        }

        Debug.LogWarning("EnemySpawner: No valid NavMesh or ground found. Using raw position.");
        return intendedPos;
    }

    // =========================================================
    // CORE SPAWN SEQUENCE
    // =========================================================

    private IEnumerator SpawnSequenceInternal(
        Vector3 spawnPos,
        Difficulty difficulty,
        GameObject prefabToSpawn,
        Color portalColor,
        System.Action<GameObject> onEnemySpawned)
    {
        // ---------- PHASE 1: PORTAL ----------
        GameObject portal = Instantiate(
            spawnPortalPrefab,
            spawnPos,
            Quaternion.Euler(90f, 0f, 0f)
        );

        Renderer portalRenderer = portal.GetComponent<Renderer>();
        ParticleSystem portalParticles = portal.GetComponentInChildren<ParticleSystem>();

        portalRenderer.material.SetColor("_Color", portalColor);

        if (portalParticles != null)
        {
            var main = portalParticles.main;
            main.startColor = portalColor;
            portalParticles.Play();
        }

        float elapsed = 0f;
        Vector3 targetScale = Vector3.one * maxPortalScale;

        while (elapsed < portalOpenTime)
        {
            elapsed += Time.deltaTime;
            float t = openCurve.Evaluate(elapsed / portalOpenTime);
            portal.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        portal.transform.localScale = targetScale;

        // ---------- PHASE 2: SPAWN ENEMY UNDERGROUND ----------
        Vector3 undergroundPos = spawnPos - Vector3.up * enemyRiseHeight;
        GameObject enemy = Instantiate(prefabToSpawn, undergroundPos, Quaternion.identity);
        enemy.tag = "Enemy";

        onEnemySpawned?.Invoke(enemy);

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();
        Material[] originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = new Material(renderers[i].material);
            SetMaterialTransparent(renderers[i].material);
            Color c = renderers[i].material.color;
            c.a = 0f;
            renderers[i].material.color = c;
        }

        yield return new WaitForSeconds(portalHoldTime);

        // ---------- PHASE 3: RISE + FADE ----------
        elapsed = 0f;
        while (elapsed < enemyRiseTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / enemyRiseTime;
            float smooth = 1f - Mathf.Pow(1f - t, 3f);

            enemy.transform.position = Vector3.Lerp(undergroundPos, spawnPos, smooth);

            float alpha = Mathf.Clamp01(t * 1.5f);
            foreach (var r in renderers)
            {
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }

            yield return null;
        }

        enemy.transform.position = spawnPos;

        // ---------- PHASE 4: ACTIVATE ENEMY ----------
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(spawnPos);
        }

        EnemyFollow follow = enemy.GetComponent<EnemyFollow>();
        if (follow != null)
            follow.Activate();

        // ---------- PHASE 5: CLOSE PORTAL ----------
        elapsed = 0f;
        while (elapsed < portalCloseTime)
        {
            elapsed += Time.deltaTime;
            float t = openCurve.Evaluate(1f - elapsed / portalCloseTime);
            portal.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        // ---------- CLEANUP ----------
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = originalMaterials[i];

        Destroy(portal);
    }

    // =========================================================
    // MATERIAL HELPERS
    // =========================================================

    private void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 2);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private Color GetDifficultyColor(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => easyColor,
            Difficulty.Medium => mediumColor,
            Difficulty.Hard => hardColor,
            _ => Color.white
        };
    }

    // =========================================================
    // TEST INPUT
    // =========================================================

    void Update()
    {
        if (autoSpawnForTesting && Time.time >= nextAutoSpawnTime)
        {
            SpawnEnemy(transform.position + transform.forward * 5f,
                (Difficulty)((int)(Time.time / autoSpawnInterval) % 3));
            nextAutoSpawnTime = Time.time + autoSpawnInterval;
        }

        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SpawnEnemy(transform.position + transform.forward * 5f, Difficulty.Easy);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SpawnEnemy(transform.position + transform.forward * 5f, Difficulty.Medium);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SpawnEnemy(transform.position + transform.forward * 5f, Difficulty.Hard);
    }
}
