using UnityEngine;

[CreateAssetMenu(fileName = "New Teleport Spell", menuName = "Spells/Teleport")]
public class TeleportSpell : Spell
{
    [Header("Teleport Settings")]
    public float maxRange = 10f;
    public float minRange = 2f;
    public LayerMask groundLayer; // What counts as ground
    public float heightOffset = 0.1f; // Slightly above ground
    
    [Header("Visual Effects")]
    public GameObject teleportIndicatorPrefab; // Shows where you'll teleport
    public GameObject departureEffectPrefab; // Effect at start position
    public GameObject arrivalEffectPrefab; // Effect at end position
    public Color validLocationColor = Color.cyan;
    public Color invalidLocationColor = Color.red;
    
    [Header("Audio")]
    public AudioClip teleportSound;
    public AudioClip invalidLocationSound;
    
    [Header("Restrictions")]
    public bool requireLineOfSight = true;
    public bool mustBeOnGround = true;
    
    private GameObject activeIndicator;
    private Vector3 targetPosition;
    private bool isValidLocation = false;

    public override void Cast(Vector3 origin, Vector3 direction)
    {
        Debug.Log($"🌀 Casting {spellName} from {origin}");

        // Find teleport destination
        if (FindTeleportLocation(origin, direction, out Vector3 destination))
        {
            PerformTeleport(origin, destination);
        }
        else
        {
            Debug.Log("❌ Invalid teleport location!");
            PlayInvalidFeedback(origin);
        }
    }

    bool FindTeleportLocation(Vector3 origin, Vector3 direction, out Vector3 destination)
    {
        destination = Vector3.zero;

        // Raycast to find target point
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxRange, groundLayer))
        {
            // Check if location is valid
            if (Vector3.Distance(origin, hit.point) < minRange)
            {
                Debug.Log($"⚠️ Too close! Min range: {minRange}m");
                return false;
            }

            // Check if it's ground
            if (mustBeOnGround && Vector3.Dot(hit.normal, Vector3.up) < 0.7f)
            {
                Debug.Log("⚠️ Not ground - too steep!");
                return false;
            }

            destination = hit.point + Vector3.up * heightOffset;
            Debug.Log($"✅ Valid teleport location: {destination}");
            return true;
        }
        else
        {
            // If no hit, teleport max range in direction
            Vector3 targetPoint = origin + direction.normalized * maxRange;
            
            // Raycast down to find ground
            if (Physics.Raycast(targetPoint + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 20f, groundLayer))
            {
                if (Vector3.Distance(origin, groundHit.point) < minRange)
                {
                    return false;
                }

                destination = groundHit.point + Vector3.up * heightOffset;
                Debug.Log($"✅ Found ground below: {destination}");
                return true;
            }
        }

        Debug.Log("❌ No valid ground found");
        return false;
    }

    void PerformTeleport(Vector3 fromPos, Vector3 toPos)
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ Cannot teleport - no Player found!");
            return;
        }

        Debug.Log($"🌀 Teleporting player from {fromPos} to {toPos}");

        // Spawn departure effect
        if (departureEffectPrefab != null)
        {
            GameObject departEffect = Instantiate(departureEffectPrefab, fromPos, Quaternion.identity);
            Destroy(departEffect, 3f);
        }

        // Teleport player
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = toPos;
            controller.enabled = true;
        }
        else
        {
            // Fallback if no CharacterController
            player.transform.position = toPos;
        }

        // Spawn arrival effect
        if (arrivalEffectPrefab != null)
        {
            GameObject arriveEffect = Instantiate(arrivalEffectPrefab, toPos, Quaternion.identity);
            Destroy(arriveEffect, 3f);
        }

        // Play sound
        if (teleportSound != null)
        {
            AudioSource.PlayClipAtPoint(teleportSound, toPos);
        }

        Debug.Log($"✅ Teleport complete!");
    }

    void PlayInvalidFeedback(Vector3 position)
    {
        if (invalidLocationSound != null)
        {
            AudioSource.PlayClipAtPoint(invalidLocationSound, position, 0.5f);
        }
    }

    // Optional: Show indicator before teleporting (for VR comfort)
    public void ShowTeleportIndicator(Vector3 origin, Vector3 direction)
    {
        if (teleportIndicatorPrefab == null) return;

        // Create indicator if doesn't exist
        if (activeIndicator == null)
        {
            activeIndicator = Instantiate(teleportIndicatorPrefab);
        }

        // Update indicator position
        if (FindTeleportLocation(origin, direction, out Vector3 destination))
        {
            activeIndicator.transform.position = destination;
            activeIndicator.SetActive(true);
            
            // Change color to valid
            Renderer renderer = activeIndicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = validLocationColor;
            }
            
            isValidLocation = true;
        }
        else
        {
            // Show invalid location
            Vector3 fallbackPos = origin + direction.normalized * maxRange;
            activeIndicator.transform.position = fallbackPos;
            activeIndicator.SetActive(true);
            
            Renderer renderer = activeIndicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = invalidLocationColor;
            }
            
            isValidLocation = false;
        }
    }

    public void HideTeleportIndicator()
    {
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(false);
        }
    }
}