using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // ADDED THIS

/// <summary>
/// Portal that appears after completing all waves
/// Walk into it to proceed to next level
/// </summary>
public class LevelPortal : MonoBehaviour
{
    [Header("Visual")]
    public float rotationSpeed = 30f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;

    [Header("Audio")]
    public AudioClip portalHumSound;
    public AudioClip portalEnterSound;
    private AudioSource audioSource;

    [Header("UI")]
    public GameObject promptUI; // Optional "Walk in to Enter" text
    public float promptDistance = 5f;
    public bool autoTeleport = true; // Set to true for collision teleport

    [Header("Effects")]
    public ParticleSystem particles;
    public Light portalLight;

    private Transform player;
    private bool playerInRange = false;
    private bool isActivated = false;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ Portal: Player not found! Make sure player has 'Player' tag.");
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f;
        audioSource.loop = true;
        audioSource.volume = 0.5f;

        if (portalHumSound != null)
        {
            audioSource.clip = portalHumSound;
            audioSource.Play();
        }

        // Setup light
        if (portalLight == null)
        {
            GameObject lightObj = new GameObject("PortalLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            portalLight = lightObj.AddComponent<Light>();
            portalLight.color = Color.cyan;
            portalLight.intensity = 3f;
            portalLight.range = 10f;
        }

        // Hide prompt initially
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        Debug.Log("✨ Level Portal spawned and ready!");
    }

    void Update()
    {
        // Rotate portal
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Pulse scale
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * (1f + pulse);

        // Pulse light intensity
        if (portalLight != null)
        {
            portalLight.intensity = 3f + Mathf.Sin(Time.time * pulseSpeed * 2f) * 1f;
        }

        // Check player distance
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= promptDistance;

            // Show/hide prompt
            if (playerInRange != wasInRange && promptUI != null)
            {
                promptUI.SetActive(playerInRange);
            }

            // Check for input - Only if NOT auto-teleport
            if (playerInRange && !isActivated && !autoTeleport)
            {
                // Check if E key was pressed using NEW Input System
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    EnterPortal();
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Auto-enter when walking into portal
        if (other.CompareTag("Player") && !isActivated)
        {
            Debug.Log("👤 Player entered portal trigger - AUTO TELEPORTING!");
            EnterPortal();
        }
    }

    void EnterPortal()
    {
        if (isActivated) return;

        isActivated = true;
        Debug.Log("🌀 Player entering portal!");

        // Play enter sound
        if (portalEnterSound != null)
        {
            AudioSource.PlayClipAtPoint(portalEnterSound, transform.position);
        }

        // Hide prompt
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        // Trigger level transition
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.CompleteLevel();
        }
        else
        {
            Debug.LogWarning("⚠️ No LevelManager found!");
        }

        // Optional: Fade out or teleport effect
        StartCoroutine(TeleportEffect());
    }

    System.Collections.IEnumerator TeleportEffect()
    {
        // Increase particle emission
        if (particles != null)
        {
            var emission = particles.emission;
            emission.rateOverTime = 100f;
        }

        // Spin faster
        float originalSpeed = rotationSpeed;
        rotationSpeed = 200f;

        // Wait a moment
        yield return new WaitForSeconds(0.5f);

        // Restore
        rotationSpeed = originalSpeed;
    }

    /// <summary>
    /// Call this to make portal appear with animation
    /// </summary>
    public void Activate()
    {
        StartCoroutine(SpawnAnimation());
    }

    System.Collections.IEnumerator SpawnAnimation()
    {
        // Start invisible
        transform.localScale = Vector3.zero;

        // Grow over 1 second
        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            transform.localScale = originalScale * progress;
            yield return null;
        }

        transform.localScale = originalScale;
        Debug.Log("✅ Portal spawn animation complete!");
    }
}