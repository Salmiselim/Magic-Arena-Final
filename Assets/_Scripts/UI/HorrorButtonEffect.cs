using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Assuming TextMeshPro, switch to Text if needed
using System.Collections;

public class HorrorButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual Effects")]
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;
    public bool enableFlicker = true;
    public float flickerIntensity = 2.0f;

    [Header("Audio Effects")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private Vector3 originalPosition;
    private bool isHovering = false;

    void Start()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            
        if (buttonText != null)
            normalColor = buttonText.color;

        originalPosition = transform.localPosition;
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Add AudioSource if missing
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        if (buttonText != null)
            buttonText.color = hoverColor;

        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);

        if (enableFlicker)
            StartCoroutine(FlickerRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        if (buttonText != null)
            buttonText.color = normalColor;
            
        // Reset position
        transform.localPosition = originalPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
            
        // Small shake on click
        StartCoroutine(ShakeRoutine(0.2f));
    }

    IEnumerator FlickerRoutine()
    {
        while (isHovering)
        {
            // Position Jitter
            float x = Random.Range(-1f, 1f) * flickerIntensity;
            float y = Random.Range(-1f, 1f) * flickerIntensity;
            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            // Alpha Jitter (Ghostly effect)
            if (buttonText != null)
            {
                float alpha = Random.Range(0.5f, 1.0f);
                Color c = hoverColor;
                c.a = alpha;
                buttonText.color = c;
            }

            yield return new WaitForSeconds(0.05f); // Fast flicker
        }
        
        // Ensure reset when loop ends
        transform.localPosition = originalPosition;
        if (buttonText != null)
            buttonText.color = normalColor;
    }
    
    IEnumerator ShakeRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localPosition = originalPosition + (Vector3)(Random.insideUnitCircle * 5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;
    }
}
