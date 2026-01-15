using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WandUISelector : MonoBehaviour
{
    [Header("Wand References")]
    public WandControllerV3 wandController;
    public Transform wandTip;
    
    [Header("UI Settings")]
    public float selectionDistance = 0.5f;
    public LayerMask uiLayer;
    public GameObject selectionCursor;
    
    private SpellSelectButton currentHoveredButton;
    private bool canSelect = true;
    
    void Update()
    {
        if (wandController == null || wandTip == null) return;
        
        // Only work if menu is open
        if (!wandController.IsMenuOpen()) 
        {
            if (selectionCursor != null && selectionCursor.activeSelf)
                selectionCursor.SetActive(false);
            return;
        }
        
        // Show cursor
        if (selectionCursor != null)
        {
            if (!selectionCursor.activeSelf)
                selectionCursor.SetActive(true);
            
            UpdateCursorPosition();
        }
        
        // Check for buttons
        CheckForButtons();
        
        // Handle selection
        if (currentHoveredButton != null && canSelect)
        {
            HandleSelectionInput();
        }
    }
    
    void UpdateCursorPosition()
    {
        if (selectionCursor != null)
        {
            selectionCursor.transform.position = wandTip.position;
            selectionCursor.transform.rotation = wandTip.rotation;
        }
    }
    
    void CheckForButtons()
    {
        Ray ray = new Ray(wandTip.position, wandTip.forward);
        RaycastHit hit;
        
        Debug.DrawRay(wandTip.position, wandTip.forward * selectionDistance, Color.green);
        
        if (Physics.Raycast(ray, out hit, selectionDistance, uiLayer))
        {
            SpellSelectButton button = hit.collider.GetComponent<SpellSelectButton>();
            
            if (button != null && button != currentHoveredButton)
            {
                // New button hovered
                if (currentHoveredButton != null)
                {
                    // Use the correct method name
                    currentHoveredButton.OnWandHoverExit();
                }
                
                currentHoveredButton = button;
                // Use the correct method name
                currentHoveredButton.OnWandHoverEnter();
            }
        }
        else if (currentHoveredButton != null)
        {
            // No button under cursor
            currentHoveredButton.OnWandHoverExit();
            currentHoveredButton = null;
        }
    }
    
    void HandleSelectionInput()
    {
        // Check if wand trigger is pressed
        bool isSelecting = CheckWandTrigger();
        
        if (isSelecting)
        {
            SelectCurrentButton();
        }
    }
    
    bool CheckWandTrigger()
    {
        // For testing - use Spacebar or Mouse
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        
        // TODO: Replace with actual wand trigger input
        // if (wandController.triggerAction != null)
        //     return wandController.triggerAction.action.ReadValue<float>() > 0.1f;
        // return false;
    }
    
    void SelectCurrentButton()
    {
        if (currentHoveredButton == null || wandController == null) return;
        
        Debug.Log($"Selected: {currentHoveredButton.GetSpellName()}");
        
        // Get the spell from the button
        Spell selectedSpell = currentHoveredButton.GetSpell();
        if (selectedSpell != null)
        {
            // Set spell on wand
            wandController.SetCurrentSpell(selectedSpell);
            
            // Also update SpellManager
            if (SpellManager.Instance != null)
            {
                SpellManager.Instance.SetActiveSpell(selectedSpell);
            }
            
            // Visual feedback on button
            currentHoveredButton.OnSelected();
            
            // Close menu
            wandController.CloseSpellMenu();
            
            // Clear hovered button
            currentHoveredButton = null;
        }
        
        // Cooldown to prevent double selection
        StartCoroutine(SelectionCooldown());
    }
    
    System.Collections.IEnumerator SelectionCooldown()
    {
        canSelect = false;
        yield return new WaitForSeconds(0.5f);
        canSelect = true;
    }
    
    // Call this when enabling/disabling the selector
    public void SetActive(bool active)
    {
        this.enabled = active;
        
        if (selectionCursor != null)
            selectionCursor.SetActive(active);
        
        if (!active && currentHoveredButton != null)
        {
            currentHoveredButton.OnWandHoverExit();
            currentHoveredButton = null;
        }
    }
}