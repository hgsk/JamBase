using UnityEngine;
// テレポーター
public class Teleporter : BaseGimmick, IInteractableGimmick
{
    public Transform destination;

    public void Interact(GameObject interactor)
    {
        if (isActive && destination != null)
        {
            interactor.transform.position = destination.position;
        }
    }
}

