using UnityEngine;

public class UIOnHandFollower : MonoBehaviour
{
    [Header("Controller Reference")]
    public Transform rightController; // Drag your right controller here
    
    [Header("Position Settings")]
    public Vector3 positionOffset = new Vector3(0.1f, 0, 0.2f); // Right and forward
    public Vector3 rotationOffset = new Vector3(0, 180, 0); // Face user
    public float followSpeed = 10f;
    
    [Header("UI Scaling")]
    public float uiScale = 0.8f; // Make UI smaller
    public float buttonSpacing = 1.2f; // Space between buttons
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        if (rightController == null)
        {
            // Try to find it automatically
            rightController = GameObject.Find("RightHand Controller")?.transform;
        }
        
        // Scale down the entire UI
        transform.localScale = Vector3.one * uiScale;
        
        // Adjust button positions if needed
        AdjustButtonLayout();
    }
    
    void Update()
    {
        if (rightController == null) return;
        
        // Calculate target position relative to controller
        targetPosition = rightController.position + 
                        rightController.right * positionOffset.x +
                        rightController.up * positionOffset.y +
                        rightController.forward * positionOffset.z;
        
        // Make UI face the opposite direction (toward user)
        targetRotation = Quaternion.LookRotation(rightController.position - targetPosition) * 
                        Quaternion.Euler(rotationOffset);
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }
    
    void AdjustButtonLayout()
    {
        // Get all buttons and arrange them in a grid
        SpellSelectButton[] buttons = GetComponentsInChildren<SpellSelectButton>();
        
        // Simple grid layout
        int columns = 3;
        float spacing = 0.3f * buttonSpacing;
        
        for (int i = 0; i < buttons.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            
            Vector3 position = new Vector3(
                (col - (columns - 1) / 2f) * spacing,
                -row * spacing,
                0
            );
            
            buttons[i].transform.localPosition = position;
        }
    }
    
    // Call this when enabling/disabling UI
    public void SetUIVisible(bool visible)
    {
        gameObject.SetActive(visible);
        
        if (visible && rightController != null)
        {
            // Snap to position immediately when showing
            targetPosition = rightController.position + 
                            rightController.right * positionOffset.x +
                            rightController.up * positionOffset.y +
                            rightController.forward * positionOffset.z;
            
            targetRotation = Quaternion.LookRotation(rightController.position - targetPosition) * 
                            Quaternion.Euler(rotationOffset);
            
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }
}