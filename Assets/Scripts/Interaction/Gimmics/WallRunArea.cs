using UnityEngine;
// 壁走り可能エリア
public class WallRunArea : BaseGimmick, IAreaGimmick
{
    public float wallRunForce = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 wallNormal = transform.up;
            Vector3 wallRunDirection = Vector3.Cross(wallNormal, Vector3.up);
            rb.AddForce(wallRunDirection * wallRunForce);
        }
    }
}

