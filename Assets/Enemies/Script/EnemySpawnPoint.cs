using UnityEngine;

/// <summary>
/// Marks a fixed location where enemies can spawn
/// Place these in your level where you want enemies to appear
/// </summary>
public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Info")]
    public int spawnPointID = 0;           // Unique ID for this spawn point
    public bool isActive = true;           // Can enemies spawn here?

    [Header("Visual (Editor Only)")]
    public Color gizmoColor = Color.red;
    public float gizmoSize = 1f;

    /// <summary>
    /// Get the spawn position (at ground level)
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        Vector3 pos = transform.position;
        pos.y = 0; // Ensure ground level
        return pos;
    }

    /// <summary>
    /// Check if this spawn point is available
    /// </summary>
    public bool IsAvailable()
    {
        return isActive && gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Disable this spawn point temporarily
    /// </summary>
    public void Disable()
    {
        isActive = false;
    }

    /// <summary>
    /// Re-enable this spawn point
    /// </summary>
    public void Enable()
    {
        isActive = true;
    }

    // Draw gizmo in Scene view so you can see spawn points
    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? gizmoColor : Color.gray;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);

        // Draw arrow pointing up
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);

        // Draw small cube at base
        Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
    }

    void OnDrawGizmosSelected()
    {
        // Draw larger sphere when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 1.5f);

        // Draw range circle on ground
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        DrawCircle(transform.position, 2f, 32);
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}