using TMPro;
using UnityEngine;

public class FPSShow : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI fpsText;

    [Header("Settings")]
    public float updateInterval = 0.5f; // How often to update the FPS text

    private float timeSinceLastUpdate;
    private int frameCount;

    void Update()
    {
        frameCount++;
        timeSinceLastUpdate += Time.unscaledDeltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            float fps = frameCount / timeSinceLastUpdate;
            fpsText.text = $"{fps:F1} FPS";

            frameCount = 0;
            timeSinceLastUpdate = 0f;
        }
    }
}
