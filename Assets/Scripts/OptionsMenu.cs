using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject optionsPanel;

    public void ToggleOptionsPanel()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void CloseOptionsPanel()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

}
