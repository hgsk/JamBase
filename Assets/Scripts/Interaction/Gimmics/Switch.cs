using UnityEngine;

public class Switch : BaseGimmick, IInteractableGimmick
{
    public bool IsActivated { get; private set; }

    public void Interact(GameObject interactor)
    {
        IsActivated = !IsActivated;
    }
}

