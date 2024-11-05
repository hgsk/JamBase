using UnityEngine;
// 重力制御装置
public class GravityController : BaseGimmick, IInteractableGimmick
{
    public Vector3 gravityDirection = Vector3.down;
    public float gravityStrength = 9.81f;

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            Physics.gravity = gravityDirection * gravityStrength;
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }
}

