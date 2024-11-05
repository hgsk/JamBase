using UnityEngine;
// 隠し通路
public class HiddenPassage : BaseGimmick, IInteractableGimmick
{
    public GameObject hiddenObject;

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            hiddenObject.SetActive(!hiddenObject.activeSelf);
        }
    }
}

