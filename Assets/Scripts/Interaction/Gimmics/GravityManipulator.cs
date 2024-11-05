using UnityEngine;
// 重力操作装置
public class GravityManipulator : BaseGimmick, IAreaGimmick
{
    public Vector3 gravityDirection = Vector3.down;
    public float gravityStrength = 9.81f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = true;
        }
    }
}

