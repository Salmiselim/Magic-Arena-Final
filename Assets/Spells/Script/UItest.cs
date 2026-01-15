using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UItest : MonoBehaviour
{
    [Header("UI Reference")]
    public Canvas uiCanvas; // Drag your UI Canvas here

    [Header("Input")]
    public InputActionReference toggleAction; // Set to Left Primary Button

    private bool isUIOpen = false;

    private void Start()
    {
        // Make sure UI is closed at start
        if (uiCanvas != null)
        {
            uiCanvas.enabled = false;
            isUIOpen = false;
        }
        else
        {
            Debug.LogError("No UI Canvas assigned to SimpleUIToggle!");
        }

        // Setup input
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnTogglePressed;
        }
        else
        {
            Debug.LogError("No Input Action assigned to SimpleUIToggle!");
        }
    }

    private void OnTogglePressed(InputAction.CallbackContext context)
    {
        if (uiCanvas == null) return;

        isUIOpen = !isUIOpen;
        uiCanvas.enabled = isUIOpen;

        Debug.Log($"UI Toggled: {(isUIOpen ? "OPEN" : "CLOSED")}");
    }

    private void OnDestroy()
    {
        // Clean up
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnTogglePressed;
        }
    }
}