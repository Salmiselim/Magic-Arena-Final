using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI deathText;
    public TextMeshProUGUI scoreText;
    public Button restartButton;
    public Button quitButton;

    [Header("Animation Settings")]
    public float fadeInDuration = 1f;
    public bool animateText = true;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Get or add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ Restart button not assigned!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ Quit button not assigned!");
        }

        // Hide initially
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show the death screen with optional score
    /// </summary>
    public void Show(int score = 0)
    {
        Debug.Log("💀 Showing death screen");

        gameObject.SetActive(true);

        // Update score if provided
        if (scoreText != null && score > 0)
        {
            scoreText.text = $"Score: {score}";
            scoreText.gameObject.SetActive(true);
        }
        else if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }

        // Fade in
        if (animateText)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            canvasGroup.alpha = 1f;
        }

        // Pause game (optional)
        // Time.timeScale = 0f; // Uncomment to pause
    }

    /// <summary>
    /// Hide the death screen
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);

        // Resume game if paused
        Time.timeScale = 1f;
    }

    System.Collections.IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled if game is paused
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    void OnRestartClicked()
    {
        Debug.Log("🔄 Restart button clicked");

        // Resume time if paused
        Time.timeScale = 1f;

        // Reload current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void OnQuitClicked()
    {
        Debug.Log("🚪 Quit button clicked");

        // Resume time if paused
        Time.timeScale = 1f;

#if UNITY_EDITOR
        // In editor, stop playing
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Editor: Stopping play mode");
#else
            // In build, quit application
            Application.Quit();
            Debug.Log("Quitting application");
#endif
    }
}