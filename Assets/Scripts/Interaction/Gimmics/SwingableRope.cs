using UnityEngine;
// スイング可能な綱
public class SwingableRope : BaseGimmick, IInteractableGimmick
{
    public float swingForce = 5f;
    public Transform ropeEnd;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 swingDirection = (ropeEnd.position - interactor.transform.position).normalized;
            rb.AddForce(swingDirection * swingForce, ForceMode.Impulse);
        }
    }
}

