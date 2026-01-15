using UnityEngine;

[CreateAssetMenu(fileName = "New Defensive Spell", menuName = "Spells/Defensive")]
public class DefensiveSpell : Spell
{
    [Header("Shield Settings")]
    public float duration = 5f;
    public float shieldRadius = 3f;
    public float pushbackForce = 15f;

    [Header("Effects")]
    public bool healsPlayer = false;
    public float healAmount = 25f;
    public bool pushEnemiesOnCast = true;
    public bool blockProjectiles = true;
    public float pushbackInterval = 0.5f; // How often to push nearby enemies

    [Header("Visual")]
    public GameObject shieldPrefab; // Optional - can be null
    public float shieldScale = 100f; // Control size here!

    public override void Cast(Vector3 origin, Vector3 direction)
    {
        Debug.Log($"🛡️ Casting {spellName} at {origin}");

        // 1. HEAL PLAYER (if enabled)
        if (healsPlayer && caster != null)
        {
            PlayerStats stats = caster.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.Heal(healAmount);
                Debug.Log($"💚 Healed player for {healAmount} HP");
            }
            else
            {
                Debug.LogWarning("⚠️ No PlayerStats found on caster");
            }
        }

        // 2. PUSH ENEMIES IMMEDIATELY (if enabled)
        if (pushEnemiesOnCast)
        {
            Vector3 center = origin;
            float pushRadius = shieldRadius;
            float force = pushbackForce * 1.5f; // Extra strong initial push

            Collider[] hits = Physics.OverlapSphere(center, pushRadius);
            int pushedCount = 0;

            Debug.Log($"💥 Pushing enemies in {pushRadius}m radius with {force} force");

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Vector3 pushDir = (hit.transform.position - center).normalized;
                    pushDir.y = 0.3f; // Add upward push

                    Rigidbody rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(pushDir * force, ForceMode.Impulse);
                        Debug.Log($"   💨 Pushed {hit.name} away");
                        pushedCount++;
                    }
                }
            }

            Debug.Log($"💨 Pushed {pushedCount} enemies");
        }

        // 3. SPAWN SHIELD VISUAL
        GameObject shield = SpawnShield(origin);

        // 4. ADD ShieldController COMPONENT (this is the key!)
        if (shield != null)
        {
            ShieldController controller = shield.AddComponent<ShieldController>();
            controller.Initialize(this, duration, shieldRadius, pushbackForce, pushbackInterval);
            Debug.Log($"🛡️ Shield active for {duration} seconds");
        }
    }

    GameObject SpawnShield(Vector3 position)
    {
        GameObject shield;

        // If custom prefab provided, use it
        if (shieldPrefab != null)
        {
            shield = Instantiate(shieldPrefab, position, Quaternion.identity);
            Debug.Log($"✨ Shield visual spawned from prefab, radius: {shieldRadius}");
        }
        else
        {
            // Create default blue sphere shield
            shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.transform.position = position;
            shield.name = "Shield";

            // Remove default collider (ShieldController adds trigger collider)
            Destroy(shield.GetComponent<Collider>());

            // Make it transparent blue
            Renderer renderer = shield.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.5f, 1f, 0.5f); // Semi-transparent blue

            // Enable transparency
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            // Add emission glow
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.3f, 0.7f, 1f) * 0.5f);

            renderer.material = mat;

            Debug.Log($"✨ Default shield visual created, radius: {shieldRadius}");
        }

        // Scale to match radius - Use shieldScale setting
        shield.transform.localScale = new Vector3(shieldScale, shieldScale, shieldScale);

        // Parent to caster so it moves with player
        if (caster != null)
        {
            shield.transform.SetParent(caster.transform);
        }

        return shield;
    }
}