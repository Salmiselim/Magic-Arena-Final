using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all spawn points in a level
/// Provides methods to get spawn locations for WaveManager
/// </summary>
public class SpawnPointManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("Auto-finds all spawn points in scene if empty")]
    public List<EnemySpawnPoint> spawnPoints = new List<EnemySpawnPoint>();

    [Header("Spawn Behavior")]
    public bool useRandomOrder = true;      // Random spawn points or sequential?
    public bool allowRepeatSpawns = true;   // Can use same spawn point multiple times?

    [Header("Debug")]
    public bool showDebugLogs = true;

    private int lastUsedIndex = -1;
    private List<int> availableIndices = new List<int>();

    // Singleton for easy access
    public static SpawnPointManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-find spawn points if list is empty
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            FindAllSpawnPoints();
        }

        InitializeIndices();
    }

    void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log($"📍 SpawnPointManager initialized with {spawnPoints.Count} spawn points");
        }
    }

    /// <summary>
    /// Find all EnemySpawnPoint objects in the scene
    /// </summary>
    void FindAllSpawnPoints()
    {
        spawnPoints = new List<EnemySpawnPoint>(FindObjectsOfType<EnemySpawnPoint>());

        if (showDebugLogs)
        {
            Debug.Log($"🔍 Auto-found {spawnPoints.Count} spawn points in scene");
        }
    }

    /// <summary>
    /// Initialize available spawn point indices
    /// </summary>
    void InitializeIndices()
    {
        availableIndices.Clear();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null && spawnPoints[i].IsAvailable())
            {
                availableIndices.Add(i);
            }
        }
    }

    /// <summary>
    /// Get a spawn position (main method used by WaveManager)
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("❌ No spawn points available!");
            return Vector3.zero;
        }

        // Refresh available indices if needed
        if (!allowRepeatSpawns && availableIndices.Count == 0)
        {
            InitializeIndices();
        }

        EnemySpawnPoint spawnPoint = GetNextSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning("⚠️ No valid spawn point found, using first one");
            return spawnPoints[0].GetSpawnPosition();
        }

        if (showDebugLogs)
        {
            Debug.Log($"📍 Using spawn point: {spawnPoint.name} at {spawnPoint.GetSpawnPosition()}");
        }

        return spawnPoint.GetSpawnPosition();
    }

    /// <summary>
    /// Get next spawn point based on settings
    /// </summary>
    EnemySpawnPoint GetNextSpawnPoint()
    {
        if (useRandomOrder)
        {
            return GetRandomSpawnPoint();
        }
        else
        {
            return GetSequentialSpawnPoint();
        }
    }

    /// <summary>
    /// Get a random spawn point
    /// </summary>
    EnemySpawnPoint GetRandomSpawnPoint()
    {
        // Get available spawn points
        List<EnemySpawnPoint> available = spawnPoints.Where(sp => sp != null && sp.IsAvailable()).ToList();

        if (available.Count == 0)
        {
            Debug.LogWarning("⚠️ No available spawn points!");
            return null;
        }

        // Pick random
        int randomIndex = Random.Range(0, available.Count);
        EnemySpawnPoint chosen = available[randomIndex];

        // Remove from available list if not allowing repeats
        if (!allowRepeatSpawns)
        {
            int originalIndex = spawnPoints.IndexOf(chosen);
            availableIndices.Remove(originalIndex);
        }

        return chosen;
    }

    /// <summary>
    /// Get spawn points in order (0, 1, 2, 3...)
    /// </summary>
    EnemySpawnPoint GetSequentialSpawnPoint()
    {
        lastUsedIndex++;

        // Loop back to start
        if (lastUsedIndex >= spawnPoints.Count)
        {
            lastUsedIndex = 0;
        }

        // Find next available
        int attempts = 0;
        while (attempts < spawnPoints.Count)
        {
            if (spawnPoints[lastUsedIndex] != null && spawnPoints[lastUsedIndex].IsAvailable())
            {
                return spawnPoints[lastUsedIndex];
            }

            lastUsedIndex = (lastUsedIndex + 1) % spawnPoints.Count;
            attempts++;
        }

        Debug.LogWarning("⚠️ No available spawn points in sequential order!");
        return null;
    }

    /// <summary>
    /// Get specific spawn point by index
    /// </summary>
    public Vector3 GetSpawnPositionByIndex(int index)
    {
        if (index < 0 || index >= spawnPoints.Count)
        {
            Debug.LogError($"❌ Invalid spawn point index: {index}");
            return Vector3.zero;
        }

        return spawnPoints[index].GetSpawnPosition();
    }

    /// <summary>
    /// Get specific spawn point by ID
    /// </summary>
    public Vector3 GetSpawnPositionByID(int id)
    {
        EnemySpawnPoint point = spawnPoints.FirstOrDefault(sp => sp.spawnPointID == id);

        if (point == null)
        {
            Debug.LogError($"❌ No spawn point found with ID: {id}");
            return Vector3.zero;
        }

        return point.GetSpawnPosition();
    }

    /// <summary>
    /// Reset all spawn points to available
    /// </summary>
    public void ResetAllSpawnPoints()
    {
        foreach (var sp in spawnPoints)
        {
            if (sp != null)
            {
                sp.Enable();
            }
        }

        InitializeIndices();
        lastUsedIndex = -1;

        if (showDebugLogs)
        {
            Debug.Log("🔄 All spawn points reset");
        }
    }

    /// <summary>
    /// Get total number of spawn points
    /// </summary>
    public int GetSpawnPointCount()
    {
        return spawnPoints.Count;
    }

    /// <summary>
    /// Visualize all spawn points in Scene view
    /// </summary>
    void OnDrawGizmos()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
            return;

        // Draw lines connecting spawn points (if sequential)
        if (!useRandomOrder)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            for (int i = 0; i < spawnPoints.Count - 1; i++)
            {
                if (spawnPoints[i] != null && spawnPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(
                        spawnPoints[i].transform.position + Vector3.up,
                        spawnPoints[i + 1].transform.position + Vector3.up
                    );
                }
            }
        }
    }
}