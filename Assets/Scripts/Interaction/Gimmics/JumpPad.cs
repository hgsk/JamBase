using UnityEngine;
// ジャンプパッド
public class JumpPad : BaseGimmick, IInteractableGimmick
{
    public float jumpForce = 10f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}

