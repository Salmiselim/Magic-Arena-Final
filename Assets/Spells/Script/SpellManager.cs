using UnityEngine;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance { get; private set; }

    public List<Spell> learnedSpells = new List<Spell>();
    private Spell activeSpell;

    // Event fired whenever the active spell changes
    public delegate void SpellChanged(Spell newSpell);
    public event SpellChanged OnSpellChanged;

    private void Awake()
    {
        // Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Optional: auto set first spell as active so Wand doesn't start null
        if (learnedSpells.Count > 0)
        {
            SetActiveSpell(learnedSpells[0]);
        }
    }

    public void LearnSpell(Spell newSpell)
    {
        if (!learnedSpells.Contains(newSpell))
        {
            learnedSpells.Add(newSpell);

            
        }
    }

    public void SetActiveSpell(Spell spell)
    {
        if (spell == null)
        {
            Debug.Log("Trying to set NULL spell?");
            return;
        }

        if (!learnedSpells.Contains(spell))
        {
            Debug.LogError($"Cannot set {spell.spellName} — player hasn't learned it.");
            return;
        }

        activeSpell = spell;
        Debug.Log("Active spell: " + spell.spellName);

        // Notify listeners (WandController, UI, etc)
        OnSpellChanged?.Invoke(activeSpell);
    }

    public Spell GetActiveSpell()
    {
        return activeSpell;
    }
}
