using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slider-based health bar that follows enemies
/// Smooth transitions, gradient colors, and camera-facing
/// IMPORTANT: This REPLACES the entire old EnemyHealthBar.cs file!
/// Delete ALL old code before pasting this.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;              // The slider component
    public Image fillImage;                  // The fill image (slider's Fill)
    private Transform target;                // The enemy this bar follows
    private Camera mainCamera;

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);  // Position above enemy
    public bool rotateToCamera = true;       // Always face camera

    [Header("Color Settings")]
    public Color fullHealthColor = new Color(0.2f, 1f, 0.2f);  // Bright green
    public Color halfHealthColor = new Color(1f, 0.8f, 0f);    // Orange/yellow
    public Color lowHealthColor = new Color(1f, 0.1f, 0.1f);   // Bright red

    [Header("Smooth Transition")]
    public bool useSmoothTransition = true;
    public float transitionSpeed = 5f;       // How fast the bar moves

    private float targetValue = 1f;          // Target fill amount
    private float currentValue = 1f;         // Current fill amount (for lerping)

    void Start()
    {
        mainCamera = Camera.main;

        // Auto-find slider if not assigned
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }

        // Auto-find fill image if not assigned
        if (fillImage == null && healthSlider != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }

        if (healthSlider == null)
        {
            Debug.LogError("❌ Health Slider not found! Make sure there's a Slider component.");
        }

        if (fillImage == null)
        {
            Debug.LogError("❌ Fill Image not found! Check slider's Fill Image.");
        }

        // Initialize slider
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            // Target destroyed, destroy health bar
            Destroy(gameObject);
            return;
        }

        // Follow the enemy with offset
        transform.position = target.position + offset;

        // Always face camera
        if (rotateToCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }

        // Smooth transition for slider value
        if (useSmoothTransition && healthSlider != null)
        {
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);
            healthSlider.value = currentValue;
        }
    }

    /// <summary>
    /// Set which enemy this health bar follows
    /// </summary>
    public void SetTarget(Transform enemyTransform)
    {
        target = enemyTransform;
        Debug.Log($"✅ Health bar set to follow: {enemyTransform.name}");
    }

    /// <summary>
    /// Update the health bar fill amount and color
    /// </summary>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider == null || fillImage == null)
        {
            Debug.LogWarning("⚠️ Health slider or fill image is null!");
            return;
        }

        // Calculate fill percentage (0 to 1)
        float healthPercent = (float)currentHealth / maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);

        // Update target value
        targetValue = healthPercent;

        // Instant update if smooth transition disabled
        if (!useSmoothTransition)
        {
            healthSlider.value = healthPercent;
            currentValue = healthPercent;
        }

        // Update color with gradient
        fillImage.color = GetHealthColor(healthPercent);

        Debug.Log($"Health bar updated: {currentHealth}/{maxHealth} ({healthPercent * 100:F0}%)");
    }

    /// <summary>
    /// Get color based on health percentage with smooth gradient
    /// </summary>
    Color GetHealthColor(float healthPercent)
    {
        if (healthPercent > 0.5f)
        {
            // Green to Yellow (100% to 50%)
            float t = (healthPercent - 0.5f) * 2f; // 0 to 1
            return Color.Lerp(halfHealthColor, fullHealthColor, t);
        }
        else
        {
            // Yellow to Red (50% to 0%)
            float t = healthPercent * 2f; // 0 to 1
            return Color.Lerp(lowHealthColor, halfHealthColor, t);
        }
    }

    /// <summary>
    /// Force immediate update (no smooth transition)
    /// </summary>
    public void SetHealthImmediate(int currentHealth, int maxHealth)
    {
        float healthPercent = (float)currentHealth / maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);

        targetValue = healthPercent;
        currentValue = healthPercent;

        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }

        if (fillImage != null)
        {
            fillImage.color = GetHealthColor(healthPercent);
        }
    }
}