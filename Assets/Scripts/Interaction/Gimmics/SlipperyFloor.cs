using UnityEngine;
// 滑る床
public class SlipperyFloor : BaseGimmick, IAreaGimmick
{
    public float frictionCoefficient = 0.1f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isActive && collision.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.AddForce(velocity.normalized * frictionCoefficient, ForceMode.Acceleration);
        }
    }
}

