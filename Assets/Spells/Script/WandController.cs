using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class WandControllerV3 : MonoBehaviour
{
    [Header("References")]
    public TrailRenderer spellTrail;
    public PlayerStats playerStats;
    public SpellManager spellManager;
    public Canvas spellMenuCanvas;
    public Transform wandTip;
    
    [Header("Input Actions")]
    public InputActionReference toggleMenuAction;
    public InputActionReference triggerAction;
    public InputActionReference selectSpellAction;   
    
    [Header("Settings")]
    public float castCooldown = 0.5f;
    public float spellSpawnOffset = 0.1f;
    public float uiInteractionRange = 2.0f;
   [Header("Audio & Events")]
public WandEventSystem wandEventSystem;
public WandAudioIntegration wandAudio;

    // Components
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    
    // State
    public bool isMenuOpen = false;
    public bool canCast = true;
    public Spell currentSpell;
    private Transform originalParent;
    public bool isHeldByRightHand = false;
    private SpellSelectButton hoveredButton;

    // Wand effects
    private List<GameObject> activeWandEffects = new List<GameObject>();

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
        
        grabInteractable.selectEntered.AddListener(OnGrabAttempt);
        grabInteractable.selectExited.AddListener(OnWandReleased);
        
        grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.throwOnDetach = false;
        grabInteractable.forceGravityOnDetach = false;
    }

    void Start()
    {
        if (spellManager != null && spellManager.learnedSpells.Count > 0)
        {
            currentSpell = spellManager.GetActiveSpell() ?? spellManager.learnedSpells[0];
            Debug.Log($"Initial spell: {currentSpell?.spellName ?? "None"}");
        }
        
        if (spellMenuCanvas != null)
            spellMenuCanvas.enabled = false;
        
        if (spellTrail != null)
            spellTrail.enabled = false;
        
        SetupInputActions();
    }
    
    void Update()
    {
        if (isMenuOpen && isHeldByRightHand && wandTip != null)
        {
            CheckUIInteraction();
        }
    }
    
   [Header("UI Settings")]
 // Increased from 1.0f
public float uiPointerRadius = 0.1f; // Wider detection
public LayerMask uiLayerMask = ~0; // Detect all layers

// Update the CheckUIInteraction method:
void CheckUIInteraction()
{
    if (!isMenuOpen || !isHeldByRightHand || wandTip == null) return;
    
    Vector3 tipPos = GetWandTipPosition();
    Vector3 tipDir = GetWandTipDirection();
    
    // Draw debug ray
    Debug.DrawRay(tipPos, tipDir * uiInteractionRange, Color.green);
    
    // Use SphereCast for wider detection
    RaycastHit[] hits = Physics.SphereCastAll(
        tipPos, 
        uiPointerRadius, 
        tipDir, 
        uiInteractionRange, 
        uiLayerMask
    );
    
    SpellSelectButton closestButton = null;
    float closestDistance = float.MaxValue;
    
    foreach (RaycastHit hit in hits)
    {
        SpellSelectButton button = hit.collider.GetComponent<SpellSelectButton>();
        if (button != null)
        {
            float distance = Vector3.Distance(tipPos, hit.point);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestButton = button;
            }
        }
    }
    
    if (closestButton != null && closestButton != hoveredButton)
    {
        // New button hovered
        if (hoveredButton != null)
            hoveredButton.OnWandHoverExit();
        
        hoveredButton = closestButton;
        hoveredButton.OnWandHoverEnter();
        Debug.Log($"Hovering over: {closestButton.GetSpellName()}");
    }
    else if (closestButton == null && hoveredButton != null)
    {
        // Lost hover
        hoveredButton.OnWandHoverExit();
        hoveredButton = null;
    }
}
    void OnGrabAttempt(SelectEnterEventArgs args)
    {
        string handName = args.interactorObject.transform.name.ToLower();
        bool isLeftHand = handName.Contains("left") || handName.Contains("_l");
        
        if (isLeftHand)
        {
            Debug.Log("Left hand blocked");
            StartCoroutine(CancelLeftHandGrab(args.interactorObject));
            return;
        }
        
        isHeldByRightHand = true;
        if (wandEventSystem != null)
    wandEventSystem.OnWandPickedUp();
        Debug.Log("Right hand grabbed wand");
        
        if (rb != null) rb.isKinematic = true;
        
        if (grabInteractable.attachTransform != null)
        {
            transform.position = grabInteractable.attachTransform.position;
            transform.rotation = grabInteractable.attachTransform.rotation;
            transform.SetParent(grabInteractable.attachTransform);
        }
    }
    
    IEnumerator CancelLeftHandGrab(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor leftHand)
    {
        yield return null;
        if (leftHand != null && leftHand.isSelectActive)
            grabInteractable.interactionManager.SelectExit(leftHand, grabInteractable);
    }
    
    void OnWandReleased(SelectExitEventArgs args)
    {
        isHeldByRightHand = false;
        if (wandEventSystem != null)
    wandEventSystem.OnWandDropped();
        transform.SetParent(originalParent);
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log("Wand released");
    }
    
    // Right Trigger
 // Left Secondary Button ← ADD THIS!

// In SetupInputActions():
void SetupInputActions()
{
    if (toggleMenuAction != null && toggleMenuAction.action != null)
    {
        toggleMenuAction.action.Enable();
        toggleMenuAction.action.performed += OnMenuToggle;
    }
    
    if (triggerAction != null && triggerAction.action != null)
    {
        triggerAction.action.Enable();
        triggerAction.action.performed += OnTriggerPressed;
        triggerAction.action.canceled += OnTriggerReleased;
    }
    
    // ADD THIS FOR SPELL SELECTION:
    if (selectSpellAction != null && selectSpellAction.action != null)
    {
        selectSpellAction.action.Enable();
        selectSpellAction.action.performed += OnSelectSpellPressed;
    }
}
void OnSelectSpellPressed(InputAction.CallbackContext context)
{
    Debug.Log("Select Spell button pressed");
    
    if (!isHeldByRightHand) 
    {
        Debug.Log("Can't select spell - wand not held");
        return;
    }
    
    // Cycle to next spell
    CycleToNextSpell();
}

void CycleToNextSpell()
{
    if (spellManager == null || spellManager.learnedSpells.Count == 0) 
    {
        Debug.Log("No spells available to cycle");
        return;
    }
    
    // Find current spell index
    int currentIndex = -1;
    for (int i = 0; i < spellManager.learnedSpells.Count; i++)
    {
        if (spellManager.learnedSpells[i] == currentSpell)
        {
            currentIndex = i;
            break;
        }
    }
    
    // Calculate next index
    int nextIndex = (currentIndex + 1) % spellManager.learnedSpells.Count;
    Spell nextSpell = spellManager.learnedSpells[nextIndex];
    
    // Set the new spell
    SetCurrentSpell(nextSpell);
    
    if (SpellManager.Instance != null)
        SpellManager.Instance.SetActiveSpell(nextSpell);
    
    Debug.Log($"Cycled to spell: {nextSpell.spellName}");
    
    // Optional: Show quick UI feedback
    ShowSpellChangeFeedback(nextSpell);
}

void ShowSpellChangeFeedback(Spell spell)
{
    // Simple debug message
    Debug.Log($"Switched to: {spell.spellName}");
    
    // Optional: Add a small floating text or particle effect
    // You could create a temporary UI element that shows the spell name
    StartCoroutine(ShowSpellNameBriefly(spell.spellName));
}

IEnumerator ShowSpellNameBriefly(string spellName)
{
    // This is optional - you can implement a proper UI later
    Debug.Log($"🔮 {spellName}");
    yield return new WaitForSeconds(1f);
}
    
    void OnMenuToggle(InputAction.CallbackContext context)
    {
        ToggleSpellMenu();
    }
    
    void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!canCast || !isHeldByRightHand) return;
        
        if (isMenuOpen)
        {
            // In menu: select button
            SelectCurrentButton();
        }
        else
        {
            // Not in menu: cast spell
            CastSpell();
        }
    }
    
    void OnTriggerReleased(InputAction.CallbackContext context)
    {
        if (spellTrail != null)
            spellTrail.enabled = false;
    }
    
    void SelectCurrentButton()
    {
        if (hoveredButton == null) return;
        
        Spell selectedSpell = hoveredButton.GetSpell();
        if (selectedSpell != null)
        {
            SetCurrentSpell(selectedSpell);
            
            if (SpellManager.Instance != null)
                SpellManager.Instance.SetActiveSpell(selectedSpell);
            
            hoveredButton.OnSelected();
            CloseSpellMenu();
            
            Debug.Log($"Selected: {selectedSpell.spellName}");
            
            hoveredButton = null;
        }
    }
    
    // ===== PUBLIC METHODS =====
    
    public void ToggleSpellMenu()
    {
        if (spellMenuCanvas == null) return;
        
        isMenuOpen = !isMenuOpen;
        if (wandAudio != null)
{
    if (isMenuOpen)
        wandAudio.PlayMenuOpen();
    else
        wandAudio.PlayMenuClose();
}
        spellMenuCanvas.enabled = isMenuOpen;
        
        if (!isMenuOpen && hoveredButton != null)
        {
            hoveredButton.OnWandHoverExit();
            hoveredButton = null;
        }
        
        Debug.Log($"Menu: {(isMenuOpen ? "OPEN" : "CLOSED")}");
    }
    
    public bool IsMenuOpen() => isMenuOpen;
    
    public void CloseSpellMenu()
    {
        isMenuOpen = false;
        if (spellMenuCanvas != null)
            spellMenuCanvas.enabled = false;
    }
    
    public void SetCurrentSpell(Spell spell)
    {
        if (wandAudio != null)
    wandAudio.PlaySpellChange();

        if (spell == null) 
        {
            Debug.LogError("Trying to set null spell!");
            return;
        }
        
        currentSpell = spell;
        Debug.Log($"Wand equipped with: {spell.spellName}");
        
        // Clear old wand effects
        ClearWandEffects();
        
        // Add visual effect to wand tip
        if (spell is ElementalMagic elemental && elemental.effectPrefab != null && wandTip != null)
        {
            GameObject wandEffect = Instantiate(elemental.effectPrefab, wandTip);
            wandEffect.transform.localPosition = Vector3.zero;
            wandEffect.transform.localRotation = Quaternion.identity;
            wandEffect.transform.localScale = Vector3.one * 0.3f;
            
            activeWandEffects.Add(wandEffect);
            Debug.Log($"Added {elemental.spellName} effect to wand tip");
        }
        
        // Update trail color
        if (spellTrail != null && spell is ElementalMagic elementalMagic)
        {
            Color elementColor = GetElementColor(elementalMagic.type);
            spellTrail.startColor = elementColor;
            spellTrail.endColor = new Color(elementColor.r, elementColor.g, elementColor.b, 0);
        }
    }
    
    void ClearWandEffects()
    {
        foreach (var effect in activeWandEffects)
        {
            if (effect != null)
                Destroy(effect);
        }
        activeWandEffects.Clear();
        
        // Also clear wand tip children
        if (wandTip != null)
        {
            foreach (Transform child in wandTip)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return new Color(1f, 0.5f, 0f);
            case ElementType.Ice: return new Color(0f, 0.8f, 1f);
            case ElementType.Lightning: return new Color(1f, 1f, 0f);
            case ElementType.Earth: return new Color(0.6f, 0.4f, 0.2f);
            case ElementType.Wind: return new Color(0.8f, 1f, 1f);
            default: return Color.white;
        }
    }
    
    public bool CanCast() => canCast && isHeldByRightHand && !isMenuOpen && currentSpell != null;
    
    public Spell GetCurrentSpell() => currentSpell;
    
    public bool IsHeldByRightHand() => isHeldByRightHand;
    
    // ===== CASTING =====
    
    void CastSpell()
    {
        if (currentSpell == null || playerStats == null) return;
        
        if (!playerStats.UseMana(currentSpell.manaCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }
        
        Vector3 spawnPos = GetWandTipPosition();
        Vector3 spawnDir = GetWandTipDirection();
        if (wandAudio != null && currentSpell != null)
{
    wandAudio.PlaySpellCast(currentSpell.spellName, GetWandTipPosition());
}
RaycastHit hit;
bool willHit = Physics.Raycast(GetWandTipPosition(), GetWandTipDirection(), out hit, 50f);
if (!willHit && wandEventSystem != null)
{
    wandEventSystem.OnSpellMiss();
}
        currentSpell.Cast(spawnPos, spawnDir);
        
        StartCoroutine(SpellCastFeedback());
        StartCoroutine(CastingCooldown());
        
        Debug.Log($"Cast: {currentSpell.spellName}");
    }
    
   public  Vector3 GetWandTipPosition()
    {
        if (wandTip != null)
            return wandTip.position + wandTip.forward * spellSpawnOffset;
        return transform.position;
    }
    
   public  Vector3 GetWandTipDirection()
    {
        if (wandTip != null)
            return wandTip.forward;
        return transform.forward;
    }
    
    IEnumerator SpellCastFeedback()
    {
        if (spellTrail != null)
        {
            spellTrail.enabled = true;
            yield return new WaitForSeconds(0.3f);
            spellTrail.enabled = false;
        }
    }
    
    IEnumerator CastingCooldown()
    {
        canCast = false;
        yield return new WaitForSeconds(castCooldown);
        canCast = true;
    }
    
    void OnDestroy()
{
    if (toggleMenuAction != null && toggleMenuAction.action != null)
        toggleMenuAction.action.performed -= OnMenuToggle;
        
    if (triggerAction != null && triggerAction.action != null)
    {
        triggerAction.action.performed -= OnTriggerPressed;
        triggerAction.action.canceled -= OnTriggerReleased;
    }
    
    // ADD THIS:
    if (selectSpellAction != null && selectSpellAction.action != null)
        selectSpellAction.action.performed -= OnSelectSpellPressed;
    
    if (grabInteractable != null)
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabAttempt);
        grabInteractable.selectExited.RemoveListener(OnWandReleased);
    }
}

    // ADD THIS METHOD to WandControllerV3.cs:
    public void VoiceCastSpell(Spell spell)
    {
        if (!CanCast())
        {
            Debug.Log("❌ Wand cannot cast right now");
            return;
        }

        if (spell == null)
        {
            Debug.LogError("❌ Trying to cast null spell");
            return;
        }

        // Save current spell
        Spell originalSpell = currentSpell;

        // Set to voice spell
        SetCurrentSpell(spell);

        // Cast it
        CastSpell();

        // Restore original spell after a tiny delay
        StartCoroutine(RestoreOriginalSpell(originalSpell, 0.1f));
    }

    private IEnumerator RestoreOriginalSpell(Spell originalSpell, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetCurrentSpell(originalSpell);
    }
}