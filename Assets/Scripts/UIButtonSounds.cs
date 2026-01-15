using UnityEngine;

public class UIButtonSounds : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickClip;
    public AudioClip hoverClip;

    public void PlayClick()
    {
        if (clickClip != null)
            audioSource.PlayOneShot(clickClip);
    }

    public void PlayHover()
    {
        if (hoverClip != null)
            audioSource.PlayOneShot(hoverClip);
    }
}
