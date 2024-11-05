using UnityEngine;
// 氷の床
public class IcyFloor : BaseGimmick, IAreaGimmick
{
    public float frictionCoefficient = 0.05f;
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
            rb.AddForce(velocity.normalized * (1f - frictionCoefficient), ForceMode.Acceleration);
        }
    }
}

