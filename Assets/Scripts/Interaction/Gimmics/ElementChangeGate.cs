using UnityEngine;
// 属性変更ゲート
public class ElementChangeGate : BaseGimmick, IInteractableGimmick
{
    public enum Element { Fire, Water, Earth, Air }
    public Element newElement;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<IElemental>(out var elemental))
        {
            elemental.ChangeElement(newElement);
        }
    }
}

