using UnityEngine;

public class CollisionTester : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🔴 COLLISION DETECTED: {other.name} | Tag: {other.tag}");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🟢 SOLID COLLISION: {collision.gameObject.name}");
    }
}