using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana;

    [Header("Death Settings")]
    public float deathDelay = 2f; // Wait before showing death screen
    public DeathScreenUI deathScreen; // Assign your death screen UI here
    public bool restartOnDeath = false; // CHANGED: Now we use death screen instead

    [Header("Knockback Settings")]
    public bool canBeKnockedBack = true;
    public float knockbackRecoverySpeed = 5f; // How fast player recovers control

    private bool isDead = false;
    private CharacterController characterController;
    private Vector3 knockbackVelocity = Vector3.zero;

    void Awake()
    {
        currentHP = maxHP;
        currentMana = maxMana;

        // Try to find CharacterController
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            // Try on parent or children
            characterController = GetComponentInParent<CharacterController>();
            if (characterController == null)
            {
                characterController = GetComponentInChildren<CharacterController>();
            }
        }

        if (characterController != null)
        {
            Debug.Log("✅ CharacterController found for knockback");
        }
        else
        {
            Debug.LogWarning("⚠️ No CharacterController found - knockback will use transform movement");
        }
    }

    void Update()
    {
        // Apply knockback gradually
        if (knockbackVelocity.magnitude > 0.1f)
        {
            ApplyKnockback();

            // Reduce knockback over time
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackRecoverySpeed * Time.deltaTime);
        }
    }

    void ApplyKnockback()
    {
        if (characterController != null)
        {
            // Use CharacterController.Move for smooth knockback
            characterController.Move(knockbackVelocity * Time.deltaTime);
        }
        else
        {
            // Fallback: direct transform movement
            transform.position += knockbackVelocity * Time.deltaTime;
        }
    }

    // -----------------------
    // Health Methods
    // -----------------------

    /// <summary>
    /// Take damage without knockback
    /// </summary>
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, Vector3.zero, 0f);
    }

    /// <summary>
    /// Take damage WITH knockback
    /// </summary>
    public void TakeDamage(float amount, Vector3 knockbackDirection, float knockbackForce)
    {
        if (isDead) return; // Can't take damage when dead

        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log("Player took " + amount + " damage. HP: " + currentHP);

        // Apply knockback if enabled
        if (canBeKnockedBack && knockbackForce > 0f)
        {
            knockbackDirection.y = 0; // Keep knockback horizontal
            knockbackDirection.Normalize();
            knockbackVelocity = knockbackDirection * knockbackForce;

            Debug.Log($"💨 Player knocked back with force {knockbackForce}");
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log("Player healed " + amount + " HP. Current HP: " + currentHP);
    }

    private void Die()
    {
        if (isDead) return; // Prevent multiple death calls

        isDead = true;

        Debug.Log("💀💀💀 PLAYER HAS DIED! 💀💀💀");

        // Stop all enemies from attacking
        StopAllEnemies();

        // Start death sequence
        StartCoroutine(DeathSequence());
    }

    void StopAllEnemies()
    {
        // Disable all enemy scripts so they stop attacking
        EnemyFollow[] meleeEnemies = FindObjectsOfType<EnemyFollow>();
        foreach (EnemyFollow enemy in meleeEnemies)
        {
            if (enemy != null)
            {
                enemy.enabled = false;
            }
        }

        // Also stop ranged enemies if you have them
        MonoBehaviour[] rangedEnemies = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in rangedEnemies)
        {
            if (script.GetType().Name == "EnemyRanged")
            {
                script.enabled = false;
            }
        }

        Debug.Log("⏸️ All enemies stopped");
    }

    IEnumerator DeathSequence()
    {
        Debug.Log($"⏳ Showing death screen in {deathDelay} seconds...");

        // Wait for death delay
        yield return new WaitForSeconds(deathDelay);

        // Show death screen instead of auto-restart
        if (deathScreen != null)
        {
            deathScreen.Show();
        }
        else
        {
            Debug.LogWarning("⚠️ No death screen assigned! Auto-restarting instead...");
            RestartLevel();
        }
    }

    void RestartLevel()
    {
        Debug.Log("🔄 RESTARTING LEVEL...");

        // Get current scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Reload it
        SceneManager.LoadScene(currentScene.name);
    }

    // -----------------------
    // Mana Methods
    // -----------------------
    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            Debug.Log("Used " + amount + " mana. Remaining: " + currentMana);
            return true;
        }
        else
        {
            Debug.Log("Not enough mana!");
            return false;
        }
    }

    public void ReplenishMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        Debug.Log("Mana replenished by " + amount + ". Current Mana: " + currentMana);
    }

    // -----------------------
    // Public Getters
    // -----------------------
    public bool IsDead()
    {
        return isDead;
    }

    public float GetHealthPercent()
    {
        return currentHP / maxHP;
    }
}