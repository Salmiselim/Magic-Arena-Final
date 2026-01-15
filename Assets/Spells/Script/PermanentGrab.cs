using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class PermanentGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }


    public void OnSelectExited(SelectExitEventArgs args)
    {
        // Only reattach if the grab wasn't canceled
        if (!args.isCanceled)
        {
            // Cast interactorObject to XRBaseInteractor
            XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;
            if (interactor == null) return;

            // Snap wand to attach transform
            grabInteractable.attachTransform.position = interactor.GetAttachTransform(grabInteractable).position;
            grabInteractable.attachTransform.rotation = interactor.GetAttachTransform(grabInteractable).rotation;

            // Start manual interaction (makes the wand stay in hand)
            interactor.StartManualInteraction(grabInteractable as IXRSelectInteractable);
        }
    }
}