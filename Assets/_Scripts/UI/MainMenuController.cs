using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Settings")]
    [Tooltip("Scene name to load when Play is clicked.")]
    public string gameSceneName = "TEST";

    [Tooltip("The main menu panel (to hide when playing if not changing scenes).")]
    public GameObject menuPanel;

    [Tooltip("The options panel to toggle.")]
    public GameObject optionsPanel;

    [Header("Audio Settings")]
    [Tooltip("Reference to the AudioSource playing background music.")]
    public AudioSource backgroundMusic;
    
    [Tooltip("Reference to the UI Slider controlling volume.")]
    public UnityEngine.UI.Slider volumeSlider;

    void Start()
    {
        if (backgroundMusic == null)
        {
            Debug.LogError("MainMenuController: Background Music AudioSource is NOT assigned in the Inspector!");
        }
        else
        {
            // Sync Slider to Audio Volume at start WITHOUT triggering OnValueChanged
            if (volumeSlider != null)
            {
                volumeSlider.SetValueWithoutNotify(backgroundMusic.volume);
            }

            if (backgroundMusic.clip == null) Debug.LogError("MainMenuController: AudioSource has no Audio Clip assigned!");

            if (!backgroundMusic.isPlaying && backgroundMusic.clip != null)
            {
                Debug.LogWarning("MainMenuController: Audio was not playing. Forcing Play() now...");
                backgroundMusic.Play();
            }
        }
    }

    public void PlayGame()
    {
        Debug.Log("MainMenuController: Play Game clicked.");
        
        // Option 1: Load a new scene
        if (!string.IsNullOrEmpty(gameSceneName) && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        // Option 2: Just hide the menu (if the game is in the same scene)
        else
        {
            if (menuPanel != null)
                menuPanel.SetActive(false);
                
            Debug.Log("MainMenuController: Hiding Menu to start game (Scene not found or empty).");
        }
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            bool isActive = optionsPanel.activeSelf;
            optionsPanel.SetActive(!isActive);
            
            // Sync slider when opening WITHOUT triggering OnValueChanged
            if (!isActive && volumeSlider != null && backgroundMusic != null)
            {
                volumeSlider.SetValueWithoutNotify(backgroundMusic.volume);
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("MainMenuController: Quit Game clicked.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Adjusts the volume of the background music.
    /// Link this to a UI Slider's OnValueChanged event.
    /// </summary>
    /// <param name="volume">Volume value from 0.0 to 1.0</param>
    public void SetVolume(float volume)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
        }
    }
}
