using UnityEngine;

/// <summary>
/// Add this to your Player GameObject
/// Receives damage from enemies and applies it to PlayerStats
/// </summary>
public class PlayerDamageReceiver : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public SpellHUD spellHUD; // Optional: for damage flash effect

    [Header("Damage Settings")]
    public bool invulnerable = false; // For testing
    public float invulnerabilityTime = 0.5f; // Brief invulnerability after hit
    private float lastDamageTime;

    [Header("Feedback")]
    public GameObject hitEffectPrefab; // Optional: spawn particles when hit
    public AudioClip hitSound; // Optional: pain sound
    public float screenShakeIntensity = 0.2f;
    public float screenShakeDuration = 0.2f;

    [Header("Debug")]
    public bool logDamage = true;

    private AudioSource audioSource;
    private Camera mainCamera;

    void Start()
    {
        // Auto-find PlayerStats if not assigned
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("❌ PlayerDamageReceiver: No PlayerStats found!");
            }
        }

        // Auto-find SpellHUD if not assigned
        if (spellHUD == null)
        {
            spellHUD = FindObjectOfType<SpellHUD>();
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get main camera for screen shake
        mainCamera = Camera.main;

        Debug.Log("✅ PlayerDamageReceiver initialized");
    }

    /// <summary>
    /// Called by enemies when they attack
    /// </summary>
    public void TakeDamage(float damage)
    {
        // Check invulnerability
        if (invulnerable)
        {
            if (logDamage)
                Debug.Log($"🛡️ Player is invulnerable - blocked {damage} damage");
            return;
        }

        // Check invulnerability cooldown
        if (Time.time < lastDamageTime + invulnerabilityTime)
        {
            if (logDamage)
                Debug.Log($"⏱️ Still in invulnerability period - blocked {damage} damage");
            return;
        }

        lastDamageTime = Time.time;

        if (logDamage)
            Debug.Log($"💔 Player taking {damage} damage!");

        // Apply damage to PlayerStats
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
        }
        else
        {
            Debug.LogError("❌ Cannot apply damage - PlayerStats is null!");
        }

        // Visual/Audio Feedback
        PlayHitFeedback();
    }

    void PlayHitFeedback()
    {
        // Flash health bar
        if (spellHUD != null)
        {
            spellHUD.FlashHealthBar();
        }

        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + Vector3.up * 1.5f; // Chest height
            GameObject effect = Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Screen shake
        if (mainCamera != null)
        {
            StartCoroutine(ScreenShake());
        }
    }

    System.Collections.IEnumerator ScreenShake()
    {
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < screenShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * screenShakeIntensity;
            float y = Random.Range(-1f, 1f) * screenShakeIntensity;

            mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPos;
    }

    /// <summary>
    /// Toggle invulnerability (for testing)
    /// </summary>
    public void SetInvulnerable(bool invuln)
    {
        invulnerable = invuln;
        Debug.Log($"Player invulnerability: {(invuln ? "ON" : "OFF")}");
    }

    void OnDrawGizmosSelected()
    {
        // Show invulnerability status in editor
        if (invulnerable)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}