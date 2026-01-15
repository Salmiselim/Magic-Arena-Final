using UnityEngine;

public class CastleDoorOpener : MonoBehaviour
{
    public Animator doorAnimator;
    public string openStateName = "Door|DoorAction";
    public float animationSpeed = 1f;

    public void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.speed = animationSpeed;
            doorAnimator.Play(openStateName, 0, 0f);
        }
    }
}
