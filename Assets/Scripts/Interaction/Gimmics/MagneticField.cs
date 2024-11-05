using UnityEngine;
// 磁力フィールド
public class MagneticField : BaseGimmick, IAreaGimmick
{
    public float magneticForce = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 directionToCenter = (transform.position - other.transform.position).normalized;
            rb.AddForce(directionToCenter * magneticForce);
        }
    }
}

