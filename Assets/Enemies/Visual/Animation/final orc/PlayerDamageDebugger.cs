using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Temporary diagnostic script to verify player damage setup
/// Attach to any GameObject in scene
/// </summary>
public class PlayerDamageDebugger : MonoBehaviour
{
    void Update()
    {
        // Press P to check player setup
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            CheckPlayerSetup();
        }

        // Press O to manually damage player (test)
        if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
        {
            TestDamagePlayer();
        }
    }

    void CheckPlayerSetup()
    {
        Debug.Log("=== PLAYER DAMAGE SETUP CHECK ===");

        // Find all GameObjects with "Player" tag
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        if (playerObjects.Length == 0)
        {
            Debug.LogError("❌ NO OBJECTS WITH 'Player' TAG FOUND!");
            Debug.LogError("   Solution: Select your player/camera and set Tag to 'Player'");
            return;
        }

        Debug.Log($"✅ Found {playerObjects.Length} object(s) with 'Player' tag:");

        foreach (GameObject obj in playerObjects)
        {
            Debug.Log($"   📍 {obj.name} at {obj.transform.position}");

            // Check for PlayerStats
            PlayerStats stats = obj.GetComponent<PlayerStats>();
            if (stats != null)
            {
                Debug.Log($"      ✅ Has PlayerStats (HP: {stats.currentHP}/{stats.maxHP})");
            }
            else
            {
                Debug.LogWarning($"      ⚠️ No PlayerStats on this object");

                // Check parent
                stats = obj.GetComponentInParent<PlayerStats>();
                if (stats != null)
                {
                    Debug.Log($"      ✅ Found PlayerStats on parent: {stats.gameObject.name}");
                }

                // Check children
                stats = obj.GetComponentInChildren<PlayerStats>();
                if (stats != null)
                {
                    Debug.Log($"      ✅ Found PlayerStats in children: {stats.gameObject.name}");
                }
            }

            // Check for collider
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                Debug.Log($"      ✅ Has Collider: {col.GetType().Name}");
                Debug.Log($"         Is Trigger: {col.isTrigger}");
            }
            else
            {
                Debug.LogWarning($"      ⚠️ No collider - projectiles won't hit!");

                col = obj.GetComponentInParent<Collider>();
                if (col != null)
                {
                    Debug.Log($"      ✅ Found Collider on parent: {col.gameObject.name}");
                }
            }
        }

        // Find PlayerStats in scene (even if not tagged)
        PlayerStats[] allStats = FindObjectsOfType<PlayerStats>();
        if (allStats.Length > 0)
        {
            Debug.Log($"\n📊 Found {allStats.Length} PlayerStats in scene:");
            foreach (PlayerStats stats in allStats)
            {
                Debug.Log($"   {stats.gameObject.name} - Tag: {stats.tag}");
            }
        }

        Debug.Log("=== END CHECK ===");
    }

    void TestDamagePlayer()
    {
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();

        if (playerStats != null)
        {
            Debug.Log("🧪 TEST: Manually damaging player by 10");
            playerStats.TakeDamage(10f);
            Debug.Log($"   Player HP now: {playerStats.currentHP}/{playerStats.maxHP}");
        }
        else
        {
            Debug.LogError("❌ Cannot test damage - no PlayerStats found!");
        }
    }
}