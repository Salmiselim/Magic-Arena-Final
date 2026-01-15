using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioSource musicSource;

    public void SetMusicEnabled(bool enabled)
    {
        if (musicSource == null) return;
        musicSource.mute = !enabled;
    }
}