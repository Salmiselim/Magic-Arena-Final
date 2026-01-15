using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages all game UI: score, wave status, level info, next wave countdown
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           // "LEVEL 1"
    public TextMeshProUGUI waveText;            // "WAVE 1/3"
    public TextMeshProUGUI scoreText;           // "SCORE: 1250"
    public TextMeshProUGUI nextWaveText;        // "Next wave in 5..."
    public TextMeshProUGUI waveCompleteText;    // "WAVE COMPLETE!"

    [Header("Score Settings")]
    public int currentScore = 0;
    public bool animateScoreIncrease = true;
    public float scoreAnimationSpeed = 20f;

    [Header("Colors")]
    public Color waveCompleteColor = Color.green;
    public Color nextWaveWarningColor = Color.yellow;
    public Color normalTextColor = Color.white;

    // Singleton
    public static GameUIManager Instance { get; private set; }

    // Static score that persists across scenes
    private static int persistentScore = 0;

    private int targetScore = 0;
    private Coroutine nextWaveCountdownCoroutine;

    void Awake()
    {
        // Don't use DontDestroyOnLoad - let each scene have its own UI
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        // Restore score from previous level
        currentScore = persistentScore;
        targetScore = persistentScore;

        // Initialize UI
        UpdateScoreDisplay();
        HideNextWaveText();
        HideWaveCompleteText();

        // Get current level from scene
        UpdateLevelDisplay();

        Debug.Log($"✅ GameUIManager initialized for {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"   Restored score: {currentScore}");
    }

    void Update()
    {
        // Animate score counting up
        if (animateScoreIncrease && currentScore < targetScore)
        {
            currentScore = Mathf.Min(
                currentScore + Mathf.CeilToInt(scoreAnimationSpeed * Time.deltaTime),
                targetScore
            );
            UpdateScoreDisplay();
        }
    }

    // =====================================================
    // SCORE SYSTEM
    // =====================================================

    /// <summary>
    /// Add score (with optional animation)
    /// </summary>
    public void AddScore(int points)
    {
        targetScore += points;
        persistentScore = targetScore; // Save for next level

        if (!animateScoreIncrease)
        {
            currentScore = targetScore;
            UpdateScoreDisplay();
        }

        Debug.Log($"💰 Score +{points} (Total: {targetScore})");
    }

    /// <summary>
    /// Set score directly (no animation)
    /// </summary>
    public void SetScore(int score)
    {
        currentScore = score;
        targetScore = score;
        persistentScore = score; // Save for next level
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Reset score (for new game)
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        targetScore = 0;
        persistentScore = 0;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {currentScore}";
        }
    }

    // =====================================================
    // LEVEL DISPLAY
    // =====================================================

    /// <summary>
    /// Update level text (called when level changes)
    /// </summary>
    public void UpdateLevelDisplay()
    {
        if (levelText != null)
        {
            int currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
            levelText.text = $"LEVEL {currentLevel - 1}";
        }
    }

    /// <summary>
    /// Set level text manually
    /// </summary>
    public void SetLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"LEVEL {level}";
        }
    }

    // =====================================================
    // WAVE DISPLAY
    // =====================================================

    /// <summary>
    /// Update wave progress (e.g., "WAVE 1/3")
    /// </summary>
    public void UpdateWaveDisplay(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"WAVE {currentWave +1 }/{totalWaves}";
            waveText.color = normalTextColor;
        }
    }

    // =====================================================
    // WAVE COMPLETE
    // =====================================================

    /// <summary>
    /// Show "WAVE COMPLETE!" message
    /// </summary>
    public void ShowWaveComplete()
    {
        if (waveCompleteText != null)
        {
            waveCompleteText.gameObject.SetActive(true);
            waveCompleteText.color = waveCompleteColor;
            StartCoroutine(HideWaveCompleteAfterDelay(2f));
        }
    }

    void HideWaveCompleteText()
    {
        if (waveCompleteText != null)
        {
            waveCompleteText.gameObject.SetActive(false);
        }
    }

    IEnumerator HideWaveCompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideWaveCompleteText();
    }

    // =====================================================
    // NEXT WAVE COUNTDOWN
    // =====================================================

    /// <summary>
    /// Start countdown to next wave (e.g., "Next wave in 5...")
    /// </summary>
    public void StartNextWaveCountdown(float seconds)
    {
        if (nextWaveCountdownCoroutine != null)
        {
            StopCoroutine(nextWaveCountdownCoroutine);
        }

        nextWaveCountdownCoroutine = StartCoroutine(NextWaveCountdownCoroutine(seconds));
    }

    IEnumerator NextWaveCountdownCoroutine(float totalSeconds)
    {
        if (nextWaveText == null) yield break;

        nextWaveText.gameObject.SetActive(true);
        nextWaveText.color = nextWaveWarningColor;

        float remaining = totalSeconds;

        while (remaining > 0)
        {
            nextWaveText.text = $"Next wave in {Mathf.CeilToInt(remaining)}...";
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        HideNextWaveText();
    }

    void HideNextWaveText()
    {
        if (nextWaveText != null)
        {
            nextWaveText.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // LEVEL COMPLETE
    // =====================================================

    /// <summary>
    /// Show "LEVEL COMPLETE!" message
    /// </summary>
    public void ShowLevelComplete()
    {
        if (waveCompleteText != null)
        {
            waveCompleteText.text = "LEVEL COMPLETE!";
            waveCompleteText.color = Color.cyan;
            waveCompleteText.gameObject.SetActive(true);
        }
    }

    // =====================================================
    // GETTERS
    // =====================================================

    public int GetCurrentScore()
    {
        return targetScore;
    }
}