using UnityEngine;
using System.Collections;

/// <summary>
/// Centralized audio manager for all game sounds
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip level3Music;
    public AudioClip victoryMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Enemy Sounds")]
    public AudioClip enemySpawn;
    public AudioClip enemyHit;
    public AudioClip enemyDeath;
    public AudioClip enemyAttack;
    public AudioClip enemyWalk; // Looping footsteps

    [Header("Combat Sounds")]
    public AudioClip batSwing;
    public AudioClip batHit;
    public AudioClip batHitCritical; // Hard hit

    [Header("Portal Sounds")]
    public AudioClip portalOpen;
    public AudioClip portalLoop; // While portal is active
    public AudioClip portalClose;

    [Header("UI Sounds")]
    public AudioClip waveStart;
    public AudioClip waveComplete;
    public AudioClip levelComplete;
    public AudioClip gameOver;

    [Header("Ambient")]
    public AudioClip ambientWind;
    public AudioClip ambientCreepy;
    [Range(0f, 1f)] public float ambientVolume = 0.3f;

    [Header("General Settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource ambientSource;
    private AudioSource[] sfxSources; // Pool of audio sources for SFX
    private int sfxSourceIndex = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioSources()
    {
        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        // Ambient source
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        ambientSource.playOnAwake = false;

        // Create pool of 10 SFX sources for playing multiple sounds
        sfxSources = new AudioSource[10];
        for (int i = 0; i < sfxSources.Length; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
            sfxSources[i].volume = sfxVolume;
        }
    }

    // ========== MUSIC ==========

    public void PlayMusic(AudioClip clip, bool fadeIn = true)
    {
        if (clip == null) return;

        if (fadeIn && musicSource.isPlaying)
        {
            StartCoroutine(CrossfadeMusic(clip));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float fadeTime = 1f;
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        // Fade out
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeTime);
            yield return null;
        }

        // Switch clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, musicVolume, elapsed / fadeTime);
            yield return null;
        }
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (fadeOut)
        {
            StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
        }
    }

    IEnumerator FadeOutMusic()
    {
        float fadeTime = 1f;
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
    }

    // ========== SFX ==========

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetNextSFXSource();
        source.volume = sfxVolume * volumeScale;
        source.PlayOneShot(clip);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeScale);
    }

    AudioSource GetNextSFXSource()
    {
        AudioSource source = sfxSources[sfxSourceIndex];
        sfxSourceIndex = (sfxSourceIndex + 1) % sfxSources.Length;
        return source;
    }

    // ========== AMBIENT ==========

    public void PlayAmbient(AudioClip clip)
    {
        if (clip == null) return;

        ambientSource.clip = clip;
        ambientSource.Play();
    }

    public void StopAmbient()
    {
        ambientSource.Stop();
    }

    // ========== CONVENIENCE METHODS ==========

    public void PlayEnemySpawn(Vector3 position) => PlaySFXAtPosition(enemySpawn, position, 0.8f);
    public void PlayEnemyHit(Vector3 position) => PlaySFXAtPosition(enemyHit, position);
    public void PlayEnemyDeath(Vector3 position) => PlaySFXAtPosition(enemyDeath, position);
    public void PlayEnemyAttack(Vector3 position) => PlaySFXAtPosition(enemyAttack, position, 0.7f);

    public void PlayBatSwing() => PlaySFX(batSwing, 0.6f);
    public void PlayBatHit(bool critical = false) => PlaySFX(critical ? batHitCritical : batHit);

    public void PlayPortalOpen(Vector3 position) => PlaySFXAtPosition(portalOpen, position);
    public void PlayPortalClose(Vector3 position) => PlaySFXAtPosition(portalClose, position);

    public void PlayWaveStart() => PlaySFX(waveStart);
    public void PlayWaveComplete() => PlaySFX(waveComplete);
    public void PlayLevelComplete() => PlaySFX(levelComplete);
    public void PlayGameOver() => PlaySFX(gameOver);

    // ========== VOLUME CONTROL ==========

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }
   

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ambientSource.volume = ambientVolume;
    }
    public void PlayMusicForScene(int sceneIndex)
    {
        AudioClip clip = null;

        switch (sceneIndex)
        {
            case 0:
                clip = menuMusic;
                break;
            case 1:
                clip = level1Music;
                break;
            case 2:
                clip = level2Music;
                break;
            case 3:
                clip = level3Music;
                break;
            default:
                clip = level1Music;
                break;
        }

        PlayMusic(clip);
    }
}