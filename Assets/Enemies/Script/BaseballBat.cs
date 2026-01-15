using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class BaseballBat : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damagePerHit = 25;
    public float hitCooldown = 0.5f;        // Prevent multiple hits too fast

    [Header("Hit Detection")]
    public float minimumVelocity = 1f;      // How fast bat must be moving to damage

    [Header("Audio (Optional)")]
    public AudioClip hitSound;

    [Header("Visual Effects (Optional)")]
    public GameObject hitEffectPrefab;      // Particle effect on hit
    public float hitEffectDuration = 1f;

    private Rigidbody rb;
    private AudioSource audioSource;
    private float lastHitTime;

    // Track velocity for hit detection
    private Vector3 previousPosition;
    private float currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // Add AudioSource if doesn't exist
        if (audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }

        previousPosition = transform.position;

        Debug.Log("⚾ Baseball Bat initialized");
    }

    void Update()
    {
        // Calculate bat velocity (speed of swing)
        currentVelocity = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check cooldown
        if (Time.time < lastHitTime + hitCooldown)
        {
            return;
        }

        // Check if bat is moving fast enough
        if (currentVelocity < minimumVelocity)
        {
            Debug.Log($"⚾ Bat too slow: {currentVelocity:F2} m/s (need {minimumVelocity})");
            return;
        }

        // Check if we hit an enemy
        EnemyFollow enemy = collision.gameObject.GetComponent<EnemyFollow>();

        if (enemy != null && enemy.IsAlive())
        {
            // Calculate damage based on swing speed (faster = more damage)
            float speedMultiplier = Mathf.Clamp(currentVelocity / 3f, 1f, 2f);
            int finalDamage = Mathf.RoundToInt(damagePerHit * speedMultiplier);

            // Deal damage
            enemy.TakeDamage(finalDamage);

            Debug.Log($"💥 BAT HIT! Velocity: {currentVelocity:F2} m/s, Damage: {finalDamage}");

            // Play hit sound
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Vector3 hitPoint = collision.contacts[0].point;
                GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
                Destroy(effect, hitEffectDuration);
            }

            // Apply impact force to enemy (optional - makes them stumble)
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 hitDirection = collision.contacts[0].normal;
                enemyRb.AddForce(-hitDirection * currentVelocity * 50f, ForceMode.Impulse);
            }

            // Haptic feedback (controller vibration)
            SendHapticFeedback();

            lastHitTime = Time.time;
        }
    }

    /// <summary>
    /// Send vibration to VR controller
    /// </summary>
    void SendHapticFeedback()
    {
        // Get the XR controller that's holding this bat
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // Get the interactor (controller) that's grabbing this
            var interactor = grabInteractable.firstInteractorSelecting;

            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
            {
                // Send haptic pulse (amplitude, duration)
                controllerInteractor.SendHapticImpulse(0.5f, 0.1f);
            }
        }
    }

    /// <summary>
    /// Visual debug in Scene view
    /// </summary>
    void OnDrawGizmos()
    {
        // Show bat velocity as a line
        Gizmos.color = currentVelocity > minimumVelocity ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (transform.position - previousPosition).normalized * 0.5f);
    }
}