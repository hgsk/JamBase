using UnityEngine;
// 巨大ファン
public class GiantFan : BaseGimmick, IAreaGimmick
{
    public float windForce = 20f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(transform.forward * windForce);
        }
    }
}

