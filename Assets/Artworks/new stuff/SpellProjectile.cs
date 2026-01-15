// SpellProjectile.cs – attach to your fireballs, light orbs, etc.
using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public Color spellColor = Color.cyan;  // set in inspector or via spell system
    public ParticleSystem impactVFX;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (impactVFX) Instantiate(impactVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}