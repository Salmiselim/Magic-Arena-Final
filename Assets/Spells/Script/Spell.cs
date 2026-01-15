using UnityEngine;

public abstract class Spell : ScriptableObject
{
    public string spellName;
    public float manaCost;
    public float cooldown;
    public float castTime;

    protected MonoBehaviour caster;

    public virtual void Initialize(MonoBehaviour casterObj)
    {
        caster = casterObj;
    }

    // Casting from position + direction (from wand)
    public abstract void Cast(Vector3 origin, Vector3 direction);
}
