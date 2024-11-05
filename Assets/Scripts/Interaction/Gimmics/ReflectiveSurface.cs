using UnityEngine;
// 反射板
public class ReflectiveSurface : BaseGimmick
{
    private void OnCollisionEnter(Collision collision)
    {
        if (isActive && collision.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 reflectedVelocity = Vector3.Reflect(rb.linearVelocity, collision.contacts[0].normal);
            rb.linearVelocity = reflectedVelocity;
        }
    }
}

