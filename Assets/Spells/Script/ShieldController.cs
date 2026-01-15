using UnityEngine;
using System.Collections;
using UnityEngine.AI;

/// <summary>
/// Controls active shield behavior - pushes enemies WITHOUT Rigidbody
/// Works with NavMeshAgent or direct transform movement
/// </summary>
public class ShieldController : MonoBehaviour
{
    private DefensiveSpell spellData;
    private float duration;
    private float radius;
    private float pushForce;
    private float pushInterval;

    private float shieldEndTime;
    private float nextPushTime;

    private SphereCollider triggerCollider;
    private Renderer shieldRenderer;

    public void Initialize(DefensiveSpell spell, float dur, float rad, float force, float interval)
    {
        spellData = spell;
        duration = dur;
        radius = rad;
        pushForce = force;
        pushInterval = interval;

        shieldEndTime = Time.time + duration;
        nextPushTime = Time.time + pushInterval;

        SetupCollider();
        SetupVisual();

        Debug.Log($"🛡️ Shield initialized - Duration: {duration}s, Radius: {radius}m");
    }

    void SetupCollider()
    {
        triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            Debug.Log("✅ Added new SphereCollider to shield");
        }
        else
        {
            Debug.Log("✅ Using existing SphereCollider on shield");
        }

        triggerCollider.isTrigger = true;
        triggerCollider.radius = 0.5f; // Works with scaled object

        Debug.Log($"✅ Shield collider setup - isTrigger: {triggerCollider.isTrigger}, radius: {triggerCollider.radius}");
    }

    void SetupVisual()
    {
        shieldRenderer = GetComponent<Renderer>();
        StartCoroutine(PulseShield());
    }

    void Update()
    {
        // Check if shield expired
        if (Time.time >= shieldEndTime)
        {
            DestroyShield();
            return;
        }

        // Periodic pushback
        if (Time.time >= nextPushTime)
        {
            PushNearbyEnemies();
            nextPushTime = Time.time + pushInterval;
        }

        // Fade out shield as it expires
        if (shieldRenderer != null)
        {
            float timeLeft = shieldEndTime - Time.time;
            if (timeLeft < 1f)
            {
                Color color = shieldRenderer.material.color;
                color.a = Mathf.Lerp(0f, 0.5f, timeLeft);
                shieldRenderer.material.color = color;
            }
        }
    }

    void PushNearbyEnemies()
    {
        Vector3 center = transform.position;
        float worldRadius = radius;

        Collider[] hits = Physics.OverlapSphere(center, worldRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                PushEnemyAway(hit.gameObject, center);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🛡️ Shield trigger ENTERED by: {other.name} (Tag: {other.tag})");

        // Block enemy projectiles
        if (other.CompareTag("Projectile") || other.name.Contains("Projectile"))
        {
            Debug.Log($"🛡️ Shield BLOCKED projectile: {other.name}");
            Destroy(other.gameObject);
            SpawnBlockEffect(other.transform.position);
            return;
        }

        // Push enemies that touch the shield
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"💥 Enemy {other.name} TOUCHED shield!");
            PushEnemyAway(other.gameObject, transform.position);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Continuously push enemies that stay inside
        if (other.CompareTag("Enemy"))
        {
            PushEnemyAway(other.gameObject, transform.position);
        }
    }

    void PushEnemyAway(GameObject enemy, Vector3 shieldCenter)
    {
        // Calculate push direction
        Vector3 pushDirection = (enemy.transform.position - shieldCenter).normalized;
        pushDirection.y = 0; // Keep on ground

        float pushDistance = pushForce * Time.deltaTime;

        // METHOD 1: Try NavMeshAgent first (if enemy uses AI navigation)
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            // Temporarily stop agent and move it
            agent.velocity = Vector3.zero;
            agent.ResetPath();

            Vector3 newPosition = enemy.transform.position + pushDirection * pushDistance;
            agent.Warp(newPosition);

            Debug.Log($"💨 Pushed {enemy.name} via NavMeshAgent");
            return;
        }

        // METHOD 2: Try CharacterController
        CharacterController controller = enemy.GetComponent<CharacterController>();
        if (controller != null && controller.enabled)
        {
            controller.Move(pushDirection * pushDistance);
            Debug.Log($"💨 Pushed {enemy.name} via CharacterController");
            return;
        }

        // METHOD 3: Try Rigidbody (if it exists)
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(pushDirection * pushForce * 10f, ForceMode.Force);
            Debug.Log($"💨 Pushed {enemy.name} via Rigidbody");
            return;
        }

        // METHOD 4: Direct Transform movement (fallback)
        enemy.transform.position += pushDirection * pushDistance;
        Debug.Log($"💨 Pushed {enemy.name} via Transform");

        // OPTIONAL: Stun enemy temporarily
        StunEnemy(enemy);
    }

    void StunEnemy(GameObject enemy)
    {
        // Disable enemy movement scripts temporarily
        EnemyFollow followScript = enemy.GetComponent<EnemyFollow>();
        if (followScript != null)
        {
            StartCoroutine(DisableScriptTemporarily(followScript, 0.5f));
        }

        // Also check for ranged enemy
        MonoBehaviour rangedScript = enemy.GetComponent("EnemyRanged") as MonoBehaviour;
        if (rangedScript != null)
        {
            StartCoroutine(DisableScriptTemporarily(rangedScript, 0.5f));
        }
    }

    IEnumerator DisableScriptTemporarily(MonoBehaviour script, float duration)
    {
        if (script == null) yield break;

        script.enabled = false;
        yield return new WaitForSeconds(duration);

        if (script != null) // Check if still exists
        {
            script.enabled = true;
        }
    }

    IEnumerator PulseShield()
    {
        if (shieldRenderer == null) yield break;

        float pulseSpeed = 2f;
        Vector3 originalScale = transform.localScale;

        while (Time.time < shieldEndTime)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.1f + 1f;
            transform.localScale = originalScale * pulse;
            yield return null;
        }
    }

    void SpawnBlockEffect(Vector3 position)
    {
        GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spark.transform.position = position;
        spark.transform.localScale = Vector3.one * 5f;

        Renderer renderer = spark.GetComponent<Renderer>();
        renderer.material.color = Color.yellow;

        Destroy(spark.GetComponent<Collider>());
        Destroy(spark, 0.3f);
    }

    void DestroyShield()
    {
        Debug.Log($"🛡️ Shield expired after {duration} seconds");

        if (shieldRenderer != null)
        {
            StartCoroutine(FlashAndDestroy());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator FlashAndDestroy()
    {
        for (int i = 0; i < 3; i++)
        {
            shieldRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            shieldRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (triggerCollider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerCollider.radius * transform.localScale.x);
        }
    }

}