using UnityEngine;
// ポータルドア
public class PortalDoor : BaseGimmick, IInteractableGimmick
{
    public Transform exitPortal;

    public void Interact(GameObject interactor)
    {
        if (isActive && exitPortal != null)
        {
            interactor.transform.position = exitPortal.position;
            interactor.transform.rotation = exitPortal.rotation;
        }
    }
}

