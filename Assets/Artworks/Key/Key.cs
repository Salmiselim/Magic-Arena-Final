// Key.cs – Attach to Key prefab root
using UnityEngine;

public class Key : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Detect door lock zone
        if (other.CompareTag("DoorLock"))
        {
            var door = other.GetComponentInParent<XRDoor_Fixed>();
           // if (door) door.UnlockWithKey();  // We'll add this method
            Destroy(gameObject);  // Key vanishes on use
        }
    }
}