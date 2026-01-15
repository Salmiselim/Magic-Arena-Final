using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [Header("Settings")]
    public float levelCompleteDelay = 3f;   // Wait 3 seconds before loading next level

    [Header("Debug Info")]
    public int currentSceneIndex;
    public string currentSceneName;

    // Singleton
    public static LevelManager Instance { get; private set; }

    void Awake()
    {
        // Don't destroy this object when loading new scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        UpdateSceneInfo();
    }

    void UpdateSceneInfo()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Current Scene: {currentSceneName} (Index: {currentSceneIndex})");
    }

    /// <summary>
    /// Called by WaveManager when all waves complete
    /// </summary>
    public void CompleteLevel()
    {
        Debug.Log("🎉 LEVEL COMPLETE! Loading next level...");
        StartCoroutine(LoadNextLevelCoroutine());
    }

    IEnumerator LoadNextLevelCoroutine()
    {
        // Wait before transitioning
        yield return new WaitForSeconds(levelCompleteDelay);

        // Calculate next scene index
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Loading scene index {nextSceneIndex}...");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("🏆 ALL LEVELS COMPLETE! Restarting from Level 1...");
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(0); // Back to first level
        }
    }

    /// <summary>
    /// Restart current level
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("🔄 Restarting level...");
        SceneManager.LoadScene(currentSceneIndex);
    }

    /// <summary>
    /// Load specific scene by index
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Loading scene {sceneIndex}...");
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"Scene index {sceneIndex} doesn't exist!");
        }
    }

    /// <summary>
    /// Load specific scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}...");
        SceneManager.LoadScene(sceneName);
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            // Press L to skip to next level (cheat)
            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                Debug.Log("⏭️ SKIP LEVEL (L pressed)");
                CompleteLevel();
            }

            // Press R to restart level
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Debug.Log("🔄 RESTART LEVEL (R pressed)");
                RestartLevel();
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneInfo();

        // Play appropriate music for this level
        PlayLevelMusic();
    }

    /// <summary>
    /// Play music based on current level
    /// </summary>
    void PlayLevelMusic()
    {
        if (AudioManager.Instance == null) return;

        AudioClip musicToPlay = null;

        // Determine which music to play based on scene index
        switch (currentSceneIndex)
        {
            case 0: // Level 1
                musicToPlay = AudioManager.Instance.level1Music;
                Debug.Log("🎵 Playing Level 1 music");
                break;
            case 1: // Level 2
                musicToPlay = AudioManager.Instance.level2Music;
                Debug.Log("🎵 Playing Level 2 music");
                break;
            case 2: // Level 3
                musicToPlay = AudioManager.Instance.level3Music;
                Debug.Log("🎵 Playing Level 3 music");
                break;
        }

        if (musicToPlay != null)
        {
            AudioManager.Instance.PlayMusic(musicToPlay, fadeIn: true);
        }
        else
        {
            Debug.LogWarning($"⚠️ No music assigned for scene index {currentSceneIndex}");
        }
    }
}