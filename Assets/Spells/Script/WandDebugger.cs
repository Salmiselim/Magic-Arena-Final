using UnityEngine;

public class WandDebugger : MonoBehaviour
{
    public WandControllerV3 wand;
    
    void Update()
    {
        // Check current state
        Debug.Log($"Wand State - Held: {wand?.IsHeldByRightHand()}, Menu: {wand?.IsMenuOpen()}, Spell: {wand?.GetCurrentSpell()?.spellName}");
        
        // Draw debug ray when menu is open
        if (wand != null && wand.IsMenuOpen() && wand.wandTip != null)
        {
            Vector3 tipPos = wand.wandTip.position;
            Vector3 tipDir = wand.wandTip.forward;
            Debug.DrawRay(tipPos, tipDir * wand.uiInteractionRange, Color.red);
        }
    }
}