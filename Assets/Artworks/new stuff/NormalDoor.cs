using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRDoor_Fixed1 : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = 100f;
    public float openSpeed = 150f;
    public float closeSpeed = 100f;

    [Header("Handle")]
    public XRSimpleInteractable handle;           // Drag your Handle here

    [Header("Key Lock (Optional)")]
    public bool requiresKey = false;              // ← Set this in Inspector
    private bool isUnlocked = false;              // Internal state

    [Header("Key Detection")]
    public Collider keyDetectionZone;             // ← Add a trigger box around lock
    public float keyDetectionRange = 0.5f;        // Extra backup sphere check

    private HingeJoint hinge;
    private JointMotor motor;
    private bool isOpen = false;

    void Awake()
    {
        hinge = GetComponentInChildren<HingeJoint>();

        // Setup limits
        hinge.useLimits = true;
        JointLimits limits = hinge.limits;
        limits.min = 0f;
        limits.max = openAngle;
        hinge.limits = limits;

        motor = hinge.motor;
        motor.force = 5000f;
        motor.freeSpin = false;
        hinge.useMotor = false;

        if (handle == null)
            handle = GetComponentInChildren<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        if (handle != null)
            handle.activated.AddListener(ToggleDoor);
    }

    void OnDisable()
    {
        if (handle != null)
            handle.activated.RemoveListener(ToggleDoor);
    }

    void Update()
    {
        // Backup: sphere check for key (more reliable than just trigger)
        if (requiresKey && !isUnlocked)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, keyDetectionRange);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Key") || hit.GetComponent<Key>() != null)
                {
                    UnlockWithKey();
                    break;
                }
            }
        }
    }

    void ToggleDoor(ActivateEventArgs args)
    {
        // If locked → give feedback and stop
        if (requiresKey && !isUnlocked)
        {
            HapticFeedback(args, 0.9f, 0.15f); // Sharp "locked" buzz
            return;
        }

        if (isOpen)
            CloseDoor(args);
        else
            OpenDoor(args);
    }

    public void OpenDoor(ActivateEventArgs args = null)
    {
        if (isOpen) return;
        isOpen = true;

        motor.targetVelocity = openSpeed;
        hinge.motor = motor;
        hinge.useMotor = true;

        HapticFeedback(args, 0.6f, 0.2f);
    }

    public void CloseDoor(ActivateEventArgs args = null)
    {
        if (!isOpen) return;
        isOpen = false;

        motor.targetVelocity = -closeSpeed;
        hinge.motor = motor;
        hinge.useMotor = true;

        HapticFeedback(args, 0.4f, 0.15f);
    }

    public void UnlockWithKey()
    {
        if (!requiresKey || isUnlocked) return;

        isUnlocked = true;
        OpenDoor(); // Auto-open when key used

        // Optional nice feedback
        if (TryGetComponent<AudioSource>(out var src))

        HapticFeedback(null, 0.8f, 0.4f); // Big success rumble
    }

    void HapticFeedback(ActivateEventArgs args, float amplitude, float duration)
    {
        // From activator hand if available
        if (args?.interactorObject?.transform?.TryGetComponent<XRController>(out var ctrl) == true)
        {
            ctrl.SendHapticImpulse(amplitude, duration);
        }
        // Or both hands as fallback
        else
        {
            foreach (var controller in FindObjectsOfType<XRController>())
                controller.SendHapticImpulse(amplitude, duration);
        }
    }

    // Optional: auto-stop motor at limits (already have in FixedUpdate from earlier)
    void FixedUpdate()
    {
        float angle = hinge.angle;
        if (isOpen && angle >= openAngle - 2f)
            hinge.useMotor = false;
        else if (!isOpen && angle <= 2f)
            hinge.useMotor = false;
    }
}