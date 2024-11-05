using UnityEngine;
// 風の流れ
public class WindStream : BaseGimmick, IAreaGimmick
{
    public Vector3 windDirection = Vector3.right;
    public float windForce = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(windDirection * windForce);
        }
    }
}

