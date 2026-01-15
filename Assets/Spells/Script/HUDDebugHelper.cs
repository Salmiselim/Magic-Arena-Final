using UnityEngine;

public class HUDDebugHelper : MonoBehaviour
{
    public Canvas hudCanvas;
    public Transform vrCamera;

    void Start()
    {
        Debug.Log("=== HUD DEBUG INFO ===");

        if (hudCanvas == null)
        {
            Debug.LogError("❌ HUD Canvas is NULL!");
            hudCanvas = GetComponent<Canvas>();
        }

        if (hudCanvas != null)
        {
            Debug.Log($"✅ Canvas found: {hudCanvas.name}");
            Debug.Log($"   Render Mode: {hudCanvas.renderMode}");
            Debug.Log($"   Enabled: {hudCanvas.enabled}");
            Debug.Log($"   Position: {hudCanvas.transform.position}");
            Debug.Log($"   Scale: {hudCanvas.transform.localScale}");

            if (hudCanvas.worldCamera != null)
            {
                Debug.Log($"   World Camera: {hudCanvas.worldCamera.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ World Camera is NULL - this might cause issues!");
            }
        }

        if (vrCamera == null)
        {
            vrCamera = Camera.main?.transform;
            if (vrCamera != null)
            {
                Debug.Log($"✅ Auto-found VR Camera: {vrCamera.name}");
            }
            else
            {
                Debug.LogError("❌ NO CAMERA FOUND!");
            }
        }
        else
        {
            Debug.Log($"✅ VR Camera assigned: {vrCamera.name}");
        }

        if (vrCamera != null)
        {
            Debug.Log($"   Camera Position: {vrCamera.position}");
            Debug.Log($"   Camera Forward: {vrCamera.forward}");
        }

        Debug.Log("===================");
    }

    void Update()
    {
        // Draw debug ray from camera
        if (vrCamera != null)
        {
            Debug.DrawRay(vrCamera.position, vrCamera.forward * 3f, Color.cyan);
        }

        // Draw line from camera to HUD
        if (vrCamera != null && hudCanvas != null)
        {
            Debug.DrawLine(vrCamera.position, hudCanvas.transform.position, Color.yellow);
        }
    }

    void OnGUI()
    {
        // Show on-screen debug info
        GUI.color = Color.yellow;
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;

        if (hudCanvas != null)
        {
            GUI.Label(new Rect(10, 10, 500, 30), $"HUD Enabled: {hudCanvas.enabled}", style);
            GUI.Label(new Rect(10, 40, 500, 30), $"HUD Pos: {hudCanvas.transform.position}", style);

            if (vrCamera != null)
            {
                float distance = Vector3.Distance(vrCamera.position, hudCanvas.transform.position);
                GUI.Label(new Rect(10, 70, 500, 30), $"Distance from Camera: {distance:F2}m", style);
            }
        }
        else
        {
            GUI.Label(new Rect(10, 10, 500, 30), "❌ HUD Canvas is NULL!", style);
        }
    }
}