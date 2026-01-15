using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public SpellManager spellManager;
    public WandControllerV3 wandController;

    [Header("Spell Display")]
    public Image spellIcon;
    public TextMeshProUGUI spellNameText;
    public TextMeshProUGUI spellManaCostText;
    
    [Header("Control Hints")]
    public TextMeshProUGUI castButtonText;
    public TextMeshProUGUI switchButtonText;
    
    [Header("Stats Bars")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;
    public Image manaBarFill;
    public TextMeshProUGUI manaText;
    
    [Header("Cooldown Overlay")]
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;
    
    [Header("Colors")]
    public Color healthColor = new Color(0.8f, 0.2f, 0.2f);
    public Color manaColor = new Color(0.2f, 0.5f, 1f);
    public Color cooldownColor = new Color(0, 0, 0, 0.7f);
    
    [Header("Element Colors")]
    public Color fireColor = new Color(1f, 0.3f, 0f);
    public Color iceColor = new Color(0f, 0.8f, 1f);
    public Color lightningColor = new Color(1f, 1f, 0f);
    public Color earthColor = new Color(0.6f, 0.4f, 0.2f);
    public Color windColor = new Color(0.8f, 1f, 1f);
    public Color offensiveColor = new Color(1f, 0.5f, 0f);
    public Color defensiveColor = new Color(0.3f, 0.6f, 1f);
    
    [Header("Spell Icons")]
    public Sprite[] spellIcons; // Array of icons matching spell order in SpellManager
    
    [Header("Low Resource Warnings")]
    public float lowHealthThreshold = 0.3f; // 30%
    public float lowManaThreshold = 0.2f; // 20%
    public Color lowHealthColor = new Color(1f, 0.1f, 0.1f);
    public Color lowManaColor = new Color(1f, 0.8f, 0f);
    
    [Header("Voice Casting Feedback")]
    public GameObject voiceCastPanel; // Optional panel that shows voice casting
    public TextMeshProUGUI voiceCastText; // Shows "🎤 FIREBALL!"
    public float voiceFeedbackDuration = 1.5f;
    
    [Header("VR Follow Settings")]
    public Transform vrCamera; // Assign your XR Camera/Main Camera
    public float followSpeed = 5f;
    public Vector3 hudOffset = new Vector3(0f, -0.2f, 1.5f); // Position relative to camera
    public bool lockYRotation = true; // Keep HUD upright
    public float tiltAngle = 15f; // Tilt HUD upward for better viewing
    
    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;
    private bool isLowHealth = false;
    private bool isLowMana = false;

    void Start()
    {
        // Find VR camera if not assigned
        if (vrCamera == null)
        {
            vrCamera = Camera.main?.transform;
            if (vrCamera == null)
            {
                Debug.LogError("❌ No VR Camera found! Assign it manually in SpellHUD.");
            }
            else
            {
                Debug.Log($"✅ SpellHUD: Auto-found camera: {vrCamera.name}");
            }
        }
        else
        {
            Debug.Log($"✅ SpellHUD: Camera assigned: {vrCamera.name}");
        }

        // Check Canvas component
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"✅ Canvas Render Mode: {canvas.renderMode}");
            Debug.Log($"✅ Canvas Enabled: {canvas.enabled}");
            Debug.Log($"✅ Canvas Position: {transform.position}");
            Debug.Log($"✅ Canvas Scale: {transform.localScale}");
            
            // For World Space canvas, set the camera
            if (canvas.renderMode == RenderMode.WorldSpace && vrCamera != null)
            {
                canvas.worldCamera = vrCamera.GetComponent<Camera>();
                Debug.Log("✅ Set world camera for canvas");
            }
        }
        else
        {
            Debug.LogError("❌ No Canvas component found!");
        }

        // Subscribe to spell change events
        if (spellManager != null)
        {
            spellManager.OnSpellChanged += UpdateSpellDisplay;
        }
        
        // Initial setup
        SetupControlHints();
        UpdateSpellDisplay(spellManager?.GetActiveSpell());
        
        // Setup bar colors
        if (healthBarFill != null)
            healthBarFill.color = healthColor;
        if (manaBarFill != null)
            manaBarFill.color = manaColor;
        if (cooldownOverlay != null)
        {
            cooldownOverlay.color = cooldownColor;
            cooldownOverlay.fillAmount = 0f;
        }
        
        // Hide voice feedback panel initially
        if (voiceCastPanel != null)
            voiceCastPanel.SetActive(false);
            
        // Initial position setup
        if (vrCamera != null)
        {
            Vector3 initialPos = vrCamera.position + vrCamera.forward * hudOffset.z 
                                                    + vrCamera.up * hudOffset.y 
                                                    + vrCamera.right * hudOffset.x;
            transform.position = initialPos;
            transform.LookAt(transform.position + vrCamera.forward);
            Debug.Log($"✅ Initial HUD position set to: {initialPos}");
        }
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateManaBar();
        UpdateCooldown();
        CheckCanCastWarning();
        FollowVRCamera(); // Keep HUD in view
    }

    void SetupControlHints()
    {
        // Display which buttons to press
        if (castButtonText != null)
            castButtonText.text = "RT - Cast";
        
        if (switchButtonText != null)
            switchButtonText.text = "Y - Switch Spell";
    }

    void UpdateSpellDisplay(Spell spell)
    {
        if (spell == null)
        {
            if (spellNameText != null)
                spellNameText.text = "No Spell";
            if (spellManaCostText != null)
                spellManaCostText.text = "0";
            if (spellIcon != null)
                spellIcon.enabled = false;
            return;
        }

        // Update spell name
        if (spellNameText != null)
        {
            spellNameText.text = spell.spellName;
        }

        // Update mana cost
        if (spellManaCostText != null)
        {
            spellManaCostText.text = spell.manaCost.ToString("F0");
        }

        // Update spell icon
        if (spellIcon != null && spellManager != null)
        {
            int spellIndex = spellManager.learnedSpells.IndexOf(spell);
            
            if (spellIndex >= 0 && spellIndex < spellIcons.Length && spellIcons[spellIndex] != null)
            {
                spellIcon.sprite = spellIcons[spellIndex];
                spellIcon.enabled = true;
            }
            else
            {
                // No icon available for this spell
                spellIcon.enabled = false;
                Debug.LogWarning($"No icon assigned for spell {spell.spellName} at index {spellIndex}");
            }
        }

        // Update spell type color
        Color typeColor = Color.white;
        
        if (spell is ElementalMagic elemental)
        {
            typeColor = GetElementColor(elemental.type);
        }
        else if (spell is OffensiveSpell)
        {
            typeColor = offensiveColor;
        }
        else if (spell is DefensiveSpell)
        {
            typeColor = defensiveColor;
        }

        // Apply color to spell name for visual feedback
        if (spellNameText != null)
        {
            spellNameText.color = typeColor;
        }
        
        // Also apply color tint to icon
        if (spellIcon != null)
        {
            spellIcon.color = typeColor;
        }
    }

    Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return fireColor;
            case ElementType.Ice: return iceColor;
            case ElementType.Lightning: return lightningColor;
            case ElementType.Earth: return earthColor;
            case ElementType.Wind: return windColor;
            default: return Color.white;
        }
    }

    void UpdateHealthBar()
    {
        if (playerStats == null || healthBarFill == null) return;

        // Calculate health percentage
        float healthPercent = playerStats.currentHP / playerStats.maxHP;
        
        // Smooth transition
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, healthPercent, Time.deltaTime * 5f);

        // Update text
        if (healthText != null)
        {
            healthText.text = $"{playerStats.currentHP:F0} / {playerStats.maxHP:F0}";
        }

        // Check for low health warning
        if (healthPercent <= lowHealthThreshold && !isLowHealth)
        {
            isLowHealth = true;
            StartCoroutine(PulseHealthBar());
        }
        else if (healthPercent > lowHealthThreshold)
        {
            isLowHealth = false;
            if (healthBarFill != null)
                healthBarFill.color = healthColor;
        }
    }

    void UpdateManaBar()
    {
        if (playerStats == null || manaBarFill == null) return;

        // Calculate mana percentage
        float manaPercent = playerStats.currentMana / playerStats.maxMana;
        
        // Smooth transition
        manaBarFill.fillAmount = Mathf.Lerp(manaBarFill.fillAmount, manaPercent, Time.deltaTime * 5f);

        // Update text
        if (manaText != null)
        {
            manaText.text = $"{playerStats.currentMana:F0} / {playerStats.maxMana:F0}";
        }

        // Check for low mana warning
        if (manaPercent <= lowManaThreshold && !isLowMana)
        {
            isLowMana = true;
            StartCoroutine(PulseManaBar());
        }
        else if (manaPercent > lowManaThreshold)
        {
            isLowMana = false;
            if (manaBarFill != null)
                manaBarFill.color = manaColor;
        }
    }

    void CheckCanCastWarning()
    {
        if (playerStats == null || spellManager == null) return;

        Spell currentSpell = spellManager.GetActiveSpell();
        if (currentSpell == null) return;

        // Check if player has enough mana for current spell
        bool canAffordSpell = playerStats.currentMana >= currentSpell.manaCost;

        // Visual feedback on mana cost text
        if (spellManaCostText != null)
        {
            if (!canAffordSpell)
            {
                // Not enough mana - make it red and pulsing
                spellManaCostText.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 2f, 1f));
            }
            else
            {
                // Enough mana - normal color
                spellManaCostText.color = manaColor;
            }
        }
    }

    void UpdateCooldown()
    {
        if (wandController == null || cooldownOverlay == null) return;

        // Check if wand is on cooldown
        if (!wandController.canCast && wandController.isHeldByRightHand)
        {
            if (!isOnCooldown)
            {
                // Start cooldown
                isOnCooldown = true;
                cooldownTimer = wandController.castCooldown;
            }

            cooldownTimer -= Time.deltaTime;
            float cooldownPercent = cooldownTimer / wandController.castCooldown;
            cooldownOverlay.fillAmount = cooldownPercent;

            if (cooldownText != null)
            {
                cooldownText.text = cooldownTimer.ToString("F1") + "s";
            }

            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownOverlay.fillAmount = 0f;
                if (cooldownText != null)
                    cooldownText.text = "";
            }
        }
        else
        {
            // Not on cooldown
            if (isOnCooldown)
            {
                isOnCooldown = false;
                cooldownOverlay.fillAmount = 0f;
                if (cooldownText != null)
                    cooldownText.text = "";
            }
        }
    }

    // Pulsing effect for low health
    System.Collections.IEnumerator PulseHealthBar()
    {
        while (isLowHealth && healthBarFill != null)
        {
            // Pulse between normal and warning color
            float t = Mathf.PingPong(Time.time * 2f, 1f);
            healthBarFill.color = Color.Lerp(healthColor, lowHealthColor, t);
            yield return null;
        }
    }

    // Pulsing effect for low mana
    System.Collections.IEnumerator PulseManaBar()
    {
        while (isLowMana && manaBarFill != null)
        {
            // Pulse between normal and warning color
            float t = Mathf.PingPong(Time.time * 2f, 1f);
            manaBarFill.color = Color.Lerp(manaColor, lowManaColor, t);
            yield return null;
        }
    }

    // Flash effect when taking damage (call this from PlayerStats.TakeDamage)
    public void FlashHealthBar()
    {
        if (healthBarFill != null)
        {
            StartCoroutine(FlashBar(healthBarFill, Color.white, healthColor));
        }
    }

    // Flash effect when not enough mana to cast (call when UseMana fails)
    public void FlashManaBar()
    {
        if (manaBarFill != null)
        {
            StartCoroutine(FlashBar(manaBarFill, Color.white, manaColor));
        }
    }

    System.Collections.IEnumerator FlashBar(Image bar, Color flashColor, Color originalColor)
    {
        bar.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        bar.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        bar.color = flashColor;
        yield return new WaitForSeconds(0.1f);
        bar.color = originalColor;
    }

    // Voice casting feedback
    public void ShowVoiceCastFeedback(string spellName)
    {
        if (voiceCastText != null)
        {
            voiceCastText.text = $"🎤 {spellName.ToUpper()}!";
        }

        if (voiceCastPanel != null)
        {
            StopCoroutine(nameof(VoiceFeedbackRoutine));
            StartCoroutine(VoiceFeedbackRoutine());
        }
    }

    System.Collections.IEnumerator VoiceFeedbackRoutine()
    {
        if (voiceCastPanel != null)
        {
            voiceCastPanel.SetActive(true);
            yield return new WaitForSeconds(voiceFeedbackDuration);
            voiceCastPanel.SetActive(false);
        }
    }

    // VR Camera following - keeps HUD always visible
    void FollowVRCamera()
    {
        if (vrCamera == null)
        {
            // Try to find camera if not assigned
            if (Camera.main != null)
            {
                vrCamera = Camera.main.transform;
                Debug.Log("✅ Found camera in Update");
            }
            return;
        }

        // Calculate target position relative to camera
        Vector3 targetPosition = vrCamera.position + vrCamera.forward * hudOffset.z 
                                                    + vrCamera.up * hudOffset.y 
                                                    + vrCamera.right * hudOffset.x;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Calculate rotation - always face the camera
        Vector3 lookDirection = vrCamera.position - transform.position;
        
        if (lockYRotation)
        {
            // Keep HUD upright (don't tilt with head)
            lookDirection.y = 0;
        }

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-lookDirection);
            
            // Add upward tilt for better viewing angle
            targetRotation *= Quaternion.Euler(tiltAngle, 0, 0);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (spellManager != null)
        {
            spellManager.OnSpellChanged -= UpdateSpellDisplay;
        }
    }
}