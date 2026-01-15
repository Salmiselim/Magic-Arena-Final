using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class portal : MonoBehaviour
{
    [Header("Next Level")]
    public string nextSceneName = "Level3";

    [Header("FX")]
    public ParticleSystem portalVFX;
    public AudioClip teleportSound;

    [Header("Activation Mode")]
    public bool useInteractable = false;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private AudioSource audioSource;
    private XRSimpleInteractable interactable;
    private Collider triggerZone;
    private bool hasTriggered = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        interactable = GetComponentInChildren<XRSimpleInteractable>();
        triggerZone = GetComponentInChildren<Collider>();

        if (triggerZone == null)
            triggerZone = GetComponent<Collider>();

        DebugLog($"Portal Awake: Interactable found? {interactable != null} | TriggerZone found? {triggerZone != null}");

        if (useInteractable)
            SetupInteractable();
        else
            SetupTrigger();
    }

    void Start()
    {
        // Additional diagnostic info
        if (triggerZone != null)
        {
            DebugLog($"Portal TriggerZone Details:");
            DebugLog($"  - Name: {triggerZone.gameObject.name}");
            DebugLog($"  - IsTrigger: {triggerZone.isTrigger}");
            DebugLog($"  - Layer: {LayerMask.LayerToName(triggerZone.gameObject.layer)}");
            DebugLog($"  - Position: {triggerZone.transform.position}");

            if (triggerZone is BoxCollider box)
                DebugLog($"  - Size: {box.size}");
            else if (triggerZone is SphereCollider sphere)
                DebugLog($"  - Radius: {sphere.radius}");
        }

        // Check for XR Rig
        GameObject xrRig = GameObject.Find("XR Origin") ?? GameObject.Find("XR Rig");
        if (xrRig != null)
        {
            DebugLog($"Found XR Rig: {xrRig.name}");
            Rigidbody rb = xrRig.GetComponent<Rigidbody>();
            DebugLog($"  - Has Rigidbody: {rb != null}");
            if (rb != null)
                DebugLog($"  - IsKinematic: {rb.isKinematic}, UseGravity: {rb.useGravity}");
            DebugLog($"  - Tag: {xrRig.tag}");
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find XR Origin or XR Rig!");
        }
    }

    void SetupInteractable()
    {
        if (interactable != null)
        {
            interactable.activated.AddListener(_ => Teleport());
            DebugLog("Interactable mode: Trigger listener added!");
        }
        else
        {
            Debug.LogError("❌ Interactable mode selected but no XRSimpleInteractable found!");
        }

        if (triggerZone != null)
            triggerZone.enabled = false;
    }

    void SetupTrigger()
    {
        if (triggerZone != null)
        {
            triggerZone.isTrigger = true;

            // CRITICAL: Add Rigidbody to the trigger zone for collision detection
            Rigidbody rb = triggerZone.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = triggerZone.gameObject.AddComponent<Rigidbody>();
                DebugLog("Added Rigidbody to TriggerZone");
            }

            // Configure rigidbody for trigger detection
            rb.isKinematic = true;
            rb.useGravity = false;

            DebugLog($"Step-on mode: Trigger enabled on {triggerZone.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ No collider found for trigger mode!");
        }

        if (interactable != null)
            interactable.enabled = false;

        DebugLog("Step-on mode: Interactable disabled!");
    }

    void OnTriggerEnter(Collider other)
    {
        DebugLog($"🎯 OnTriggerEnter called! Object: {other.name}, Tag: {other.tag}");

        if (useInteractable)
        {
            DebugLog("Using interactable mode, ignoring trigger");
            return;
        }

        if (hasTriggered)
        {
            DebugLog("Already triggered, ignoring");
            return;
        }

        bool isPlayer = IsPlayerObject(other);

        if (isPlayer)
        {
            DebugLog("✅ PLAYER DETECTED – TELEPORTING!");
            Teleport();
        }
        else
        {
            DebugLog($"⏭️ Not player - ignoring {other.name}");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Backup detection if trigger doesn't work
        DebugLog($"💥 OnCollisionEnter called! Object: {collision.gameObject.name}");

        if (!triggerZone.isTrigger)
        {
            DebugLog("⚠️ WARNING: Collider is not set as trigger! Using collision instead.");
            OnTriggerEnter(collision.collider);
        }
    }

    bool IsPlayerObject(Collider other)
    {
        // Method 1: Direct tag check
        if (other.CompareTag("Player"))
        {
            DebugLog("   ✓ Matched by Player tag");
            return true;
        }

        // Method 2: Check parents for Player tag
        Transform current = other.transform;
        int depth = 0;
        while (current != null && depth < 10)
        {
            if (current.CompareTag("Player"))
            {
                DebugLog($"   ✓ Matched by parent Player tag: {current.name}");
                return true;
            }
            current = current.parent;
            depth++;
        }

        // Method 3: Check for XR components
        if (other.GetComponentInParent<CharacterController>() != null)
        {
            DebugLog("   ✓ Matched by CharacterController");
            return true;
        }

        var locomotion = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>();
        if (locomotion != null)
        {
            DebugLog("   ✓ Matched by LocomotionProvider");
            return true;
        }

        // Method 4: Check by name (fallback)
        string nameLower = other.name.ToLower();
        if (nameLower.Contains("player") ||
            nameLower.Contains("xr origin") ||
            nameLower.Contains("xr rig") ||
            nameLower.Contains("camera offset") ||
            nameLower.Contains("main camera"))
        {
            DebugLog($"   ✓ Matched by name: {other.name}");
            return true;
        }

        DebugLog($"   ✗ No match found for {other.name}");
        return false;
    }

    public void Teleport()
    {
        if (hasTriggered)
        {
            DebugLog("Already triggered, ignoring duplicate call");
            return;
        }

        hasTriggered = true;
        DebugLog("🌀 Teleport called – loading " + nextSceneName);
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        if (portalVFX != null)
            portalVFX.Play();

        if (teleportSound != null && audioSource != null)
            audioSource.PlayOneShot(teleportSound);

        yield return new WaitForSeconds(0.5f);

        DebugLog("📦 LOADING SCENE: " + nextSceneName);

        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"❌ Scene '{nextSceneName}' not found in Build Settings!");
            Debug.LogError("Add it to File > Build Settings > Add Open Scenes");
            hasTriggered = false;
        }
    }

    void DebugLog(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[Portal] {message}");
    }

    void OnDrawGizmos()
    {
        Collider col = GetComponentInChildren<Collider>();
        if (col == null) col = GetComponent<Collider>();

        if (col != null)
        {
            Gizmos.color = useInteractable ? Color.blue : Color.green;
            Gizmos.matrix = col.transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.color = new Color(0, 1, 0, 0.1f);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                Gizmos.color = new Color(0, 1, 0, 0.1f);
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}