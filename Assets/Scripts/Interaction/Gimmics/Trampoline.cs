using UnityEngine;
// トランポリン
public class Trampoline : BaseGimmick, IInteractableGimmick
{
    public float bounceForce = 20f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
        }
    }
}

