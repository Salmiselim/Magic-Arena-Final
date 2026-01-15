using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandEventSystem : MonoBehaviour
{
    [Header("References")]
    public WandControllerV3 wandController;
    public PlayerStats playerStats;
    public WandAudioIntegration wandAudio;

    [Header("Settings")]
    [Range(0f, 1f)] public float lowManaThreshold = 0.25f;
    [Range(0f, 1f)] public float lowHealthThreshold = 0.3f;
    public float healthManaCheckInterval = 2f;
    
    [Header("Movement")]
    public float nauseousVelocityThreshold = 10f;
    public float nauseousTimeThreshold = 3f;
    public float nauseousCooldown = 10f; 
    
    [Header("Combat")]
    public int consecutiveMissesThreshold = 3;
    public float missTimeWindow = 5f;
    [Header("Wand Recall Settings")]
public float recallCheckInterval = 10f; // How often to check if player is away
public float minRecallDistance = 3f; // Minimum distance to trigger recall
public float recallCooldown = 30f; // Don't spam the recall voice
private float lastRecallTime = -999f;
private Transform playerTransform;
    private Vector3 lastWandTipPosition;
    private Quaternion lastWandTipRotation;
    private Vector3 lastAngularVelocity;
    private float currentAngularVelocity;
    private float lastNauseousTime = -999f;
    // State
    private bool lowManaWarningGiven = false;
    private bool lowHealthWarningGiven = false;
    private int consecutiveMisses = 0;
    private float lastMissTime = 0f;
    private float fastMovementTimer = 0f;
    private Vector3 lastPosition;
    private bool wasBeingAttacked = false;

    void Start()
    {
        if (wandController == null)
            wandController = GetComponent<WandControllerV3>();
            
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
            
        if (wandAudio == null)
            wandAudio = GetComponent<WandAudioIntegration>();
            
        lastPosition = transform.position;
          // Find player transform for distance checks
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
        playerTransform = player.transform;
    
    // Start recall monitoring
    if (playerTransform != null)
        StartCoroutine(MonitorWandRecall());
        StartCoroutine(MonitorHealthMana());
    }
// Add this new coroutine:
IEnumerator MonitorWandRecall()
{
    while (true)
    {
        yield return new WaitForSeconds(recallCheckInterval);
        
        // Only check if wand is NOT held and enough time has passed
        if (!wandController.IsHeldByRightHand() && 
            Time.time - lastRecallTime >= recallCooldown)
        {
            CheckIfPlayerIsAwayFromWand();
        }
    }
}

void CheckIfPlayerIsAwayFromWand()
{
    if (playerTransform == null) return;
    
    float distance = Vector3.Distance(playerTransform.position, transform.position);
    
    if (distance >= minRecallDistance)
    {
        TriggerWandRecall();
        lastRecallTime = Time.time;
    }
}

void TriggerWandRecall()
{
    if (wandAudio != null)
    {
        wandAudio.PlayWandRecall();
        Debug.Log($"📢 Wand recall triggered! Player is {Vector3.Distance(playerTransform.position, transform.position):F1}m away");
    }
}

// Add this public method to trigger manually if needed:
public void ForceWandRecall()
{
    TriggerWandRecall();
    lastRecallTime = Time.time;
}
    void Update()
    {
        if (wandController.IsHeldByRightHand())
        {
            MonitorFastMovement();
        }
    }

    // ===== AUTOMATED MONITORS =====
    IEnumerator MonitorHealthMana()
    {
        while (true)
        {
            yield return new WaitForSeconds(healthManaCheckInterval);
            
            if (playerStats == null) continue;
            
            // Check mana
            float manaPercent = playerStats.currentMana / playerStats.maxMana;
            if (manaPercent <= lowManaThreshold && !lowManaWarningGiven && wandAudio != null)
            {
                wandAudio.PlayLowMana();
                lowManaWarningGiven = true;
            }
            else if (manaPercent > lowManaThreshold * 1.5f)
            {
                lowManaWarningGiven = false;
            }
            
            // Check health
            float healthPercent = playerStats.currentHP / playerStats.maxHP;
            if (healthPercent <= lowHealthThreshold && !lowHealthWarningGiven && wandAudio != null)
            {
                wandAudio.PlayLowHealth();
                lowHealthWarningGiven = true;
            }
            else if (healthPercent > lowHealthThreshold * 1.5f)
            {
                lowHealthWarningGiven = false;
            }
        }
    }
void MonitorNauseousMovement()
{
    if (wandController == null || wandController.wandTip == null) return;
    
    // Check cooldown
    if (Time.time - lastNauseousTime < nauseousCooldown) return;
    
    Transform wandTip = wandController.wandTip;
    
    // Calculate velocity of wand tip (not the wand base)
    Vector3 velocity = (wandTip.position - lastWandTipPosition) / Time.deltaTime;
    float speed = velocity.magnitude;
    
    // Store for next frame
    lastWandTipPosition = wandTip.position;
    
    // Check for fast movement (whipping motion)
    if (speed > nauseousVelocityThreshold)
    {
        fastMovementTimer += Time.deltaTime;
        
        Debug.Log($"🌀 Wand tip speed: {speed:F1} m/s, Timer: {fastMovementTimer:F1}/{nauseousTimeThreshold}s");
        
        if (fastMovementTimer >= nauseousTimeThreshold)
        {
            Debug.Log("🤢 Triggering nauseous voice - wand whipping too fast!");
            PlayNauseous();
            fastMovementTimer = 0f;
            lastNauseousTime = Time.time;
        }
    }
    else
    {
        fastMovementTimer = Mathf.Max(0f, fastMovementTimer - Time.deltaTime);
    }
}
    void MonitorFastMovement()
    {
        float velocity = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        
        if (velocity > nauseousVelocityThreshold)
        {
            fastMovementTimer += Time.deltaTime;
            
            if (fastMovementTimer >= nauseousTimeThreshold && wandAudio != null)
            {
                wandAudio.PlayNauseous();
                fastMovementTimer = 0f;
            }
        }
        else
        {
            fastMovementTimer = Mathf.Max(0f, fastMovementTimer - Time.deltaTime);
        }
        
        lastPosition = transform.position;
    }

    // ===== PUBLIC METHODS (call these from other scripts) =====
    public void OnWandPickedUp()
    {
        if (wandAudio != null)
            wandAudio.PlayWandPickup();
    }

    public void OnWandDropped()
    {
        if (wandAudio != null)
            wandAudio.PlayWandDrop();
    }

    public void OnSpellMiss()
    {
        if (Time.time - lastMissTime > missTimeWindow)
            consecutiveMisses = 1;
        else
            consecutiveMisses++;
        
        lastMissTime = Time.time;
        
        if (consecutiveMisses >= consecutiveMissesThreshold && wandAudio != null)
        {
            wandAudio.PlayYouMissed();
            consecutiveMisses = 0;
        }
    }

    public void OnPlayerAttacked()
    {
        if (!wasBeingAttacked && wandAudio != null)
        {
            wandAudio.PlayWatchOut();
            wasBeingAttacked = true;
            Invoke(nameof(ResetAttackState), 5f);
        }
    }

    public void OnWaveStart(int waveNumber)
    {
        if (wandAudio != null)
            wandAudio.PlayNewWave();
    }
    void PlayNauseous()
    {

        if (wandController == null || wandController.wandTip == null) return;

        // Check cooldown
        if (Time.time - lastNauseousTime < nauseousCooldown) return;

        Transform wandTip = wandController.wandTip;

        // Calculate velocity of wand tip (not the wand base)
        Vector3 velocity = (wandTip.position - lastWandTipPosition) / Time.deltaTime;
        float speed = velocity.magnitude;

        // Store for next frame
        lastWandTipPosition = wandTip.position;

        // Check for fast movement (whipping motion)
        if (speed > nauseousVelocityThreshold)
        {
            fastMovementTimer += Time.deltaTime;

            Debug.Log($"🌀 Wand tip speed: {speed:F1} m/s, Timer: {fastMovementTimer:F1}/{nauseousTimeThreshold}s");

            if (fastMovementTimer >= nauseousTimeThreshold)
            {
                Debug.Log("🤢 Triggering nauseous voice - wand whipping too fast!");
                PlayNauseous();
                fastMovementTimer = 0f;
                lastNauseousTime = Time.time;
            }
        }
        else
        {
            fastMovementTimer = Mathf.Max(0f, fastMovementTimer - Time.deltaTime);
        }
    }
    
    public void OnPauseMenuToggle(bool isPaused)
    {
        if (wandAudio != null)
        {
            if (isPaused)
                wandAudio.PlayPauseEnter();
            else
                wandAudio.PlayPauseExit();
        }
    }

    public void OnLevelComplete(int level)
    {
        if (wandAudio != null)
            wandAudio.PlayLevelAdvanced();
    }

    public void OnPlayerDeath()
    {
        if (wandAudio != null)
            wandAudio.PlayYouLose();
    }

    public void ReportWrongDirection()
    {
        if (wandAudio != null && Random.Range(0, 100) < 10)
            wandAudio.PlayWrongDirection();
    }

    public void TriggerTutorialKill()
    {
        if (wandAudio != null)
            wandAudio.PlayTutorialKill();
    }

    public void TriggerTutorialSpellChange()
    {
        if (wandAudio != null)
            wandAudio.PlayTutorialSpellChange();
    }

    public void TriggerTutorialAim()
    {
        if (wandAudio != null)
            wandAudio.PlayTutorialAim();
    }

    public void TriggerTutorialPickup()
    {
        if (wandAudio != null)
            wandAudio.PlayTutorialPickup();
    }

    private void ResetAttackState()
    {
        wasBeingAttacked = false;
    }
}