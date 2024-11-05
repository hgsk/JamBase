using UnityEngine;
// 一方通行の壁
public class OneWayWall : BaseGimmick
{
    public Vector3 allowedDirection = Vector3.right;

    private void OnTriggerEnter(Collider other)
    {
        if (isActive)
        {
            Vector3 enterDirection = other.transform.position - transform.position;
            if (Vector3.Dot(enterDirection, allowedDirection) < 0)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), other, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), other, false);
    }
}

