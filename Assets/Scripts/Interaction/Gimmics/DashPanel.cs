using UnityEngine;
// ダッシュパネル
public class DashPanel : BaseGimmick, IInteractableGimmick
{
    public float dashForce = 20f;
    public Vector3 dashDirection = Vector3.forward;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        }
    }
}

