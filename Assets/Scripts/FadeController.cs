using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 3f;
    public float delayBeforeFade = 1f;    // time to let the door open first
    public string nextSceneName = "Level1";

    bool isFading;
    
    void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public void FadeAndLoad()
    {
        if (!isFading)
            StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeOutAndLoad()
    {
        isFading = true;

        // wait so the player can see the door animation
        yield return new WaitForSeconds(delayBeforeFade);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        SceneManager.LoadScene(nextSceneName);
    }
}
