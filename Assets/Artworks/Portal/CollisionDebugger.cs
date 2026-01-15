using UnityEngine;

// Add this to your XR Origin/XR Rig temporarily
public class CollisionDebugger : MonoBehaviour
{
    void Start()
    {
        // Log all colliders in this hierarchy
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Debug.Log($"=== XR Rig has {colliders.Length} colliders ===");

        foreach (Collider col in colliders)
        {
            Debug.Log($"  • {col.gameObject.name}: {col.GetType().Name}, IsTrigger={col.isTrigger}, Layer={LayerMask.LayerToName(col.gameObject.layer)}");
        }

        // Check for rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log($"XR Rig Rigidbody: {(rb != null ? "YES" : "NO")}");
        if (rb != null)
        {
            Debug.Log($"  IsKinematic={rb.isKinematic}, UseGravity={rb.useGravity}");
        }

        // Check tag
        Debug.Log($"XR Rig Tag: {gameObject.tag}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[XR Rig] Trigger entered: {other.name}");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[XR Rig] Collision entered: {collision.gameObject.name}");
    }
}