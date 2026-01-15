using UnityEngine;

public class SpellDebugger : MonoBehaviour
{
    public WandControllerV3 wand;
    private Spell lastWandSpell;
    private Spell lastManagerSpell;
    
    void Start()
    {
        if (wand != null)
        {
            lastWandSpell = wand.GetCurrentSpell();
            Debug.Log($"Initial wand spell: {lastWandSpell?.spellName ?? "NONE"}");
        }
        
        if (SpellManager.Instance != null)
        {
            SpellManager.Instance.OnSpellChanged += OnSpellManagerChanged;
            lastManagerSpell = SpellManager.Instance.GetActiveSpell();
            Debug.Log($"Initial SpellManager spell: {lastManagerSpell?.spellName ?? "NONE"}");
            
            Debug.Log($"Learned spells ({SpellManager.Instance.learnedSpells.Count}):");
            for (int i = 0; i < SpellManager.Instance.learnedSpells.Count; i++)
            {
                Debug.Log($"  [{i}] {SpellManager.Instance.learnedSpells[i].spellName}");
            }
        }
    }
    
    void Update()
    {
        if (wand != null)
        {
            Spell currentWandSpell = wand.GetCurrentSpell();
            if (currentWandSpell != lastWandSpell)
            {
                Debug.Log($"Wand spell CHANGED: {lastWandSpell?.spellName ?? "NONE"} → {(currentWandSpell?.spellName ?? "NONE")}");
                lastWandSpell = currentWandSpell;
            }
        }
    }
    
    void OnSpellManagerChanged(Spell newSpell)
    {
        Debug.Log($"SpellManager spell CHANGED: {lastManagerSpell?.spellName ?? "NONE"} → {newSpell?.spellName ?? "NONE"}");
        lastManagerSpell = newSpell;
    }
    
    void OnDestroy()
    {
        if (SpellManager.Instance != null)
        {
            SpellManager.Instance.OnSpellChanged -= OnSpellManagerChanged;
        }
    }
    
    // Optional: Add UI button to manually trigger debug
    public void ManualDebug()
    {
        Debug.Log("=== MANUAL SPELL DEBUG ===");
        Debug.Log($"Wand current spell: {(wand?.GetCurrentSpell()?.spellName ?? "NONE")}");
        Debug.Log($"Wand held: {wand?.IsHeldByRightHand()}");
        Debug.Log($"Wand can cast: {wand?.CanCast()}");
        Debug.Log($"Menu open: {wand?.IsMenuOpen()}");
        
        if (SpellManager.Instance != null)
        {
            Debug.Log($"SpellManager active: {SpellManager.Instance.GetActiveSpell()?.spellName ?? "NONE"}");
        }
        Debug.Log("==========================");
    }
}