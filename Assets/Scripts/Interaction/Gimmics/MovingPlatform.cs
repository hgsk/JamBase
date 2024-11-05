using UnityEngine;
// 移動プラットフォーム
public class MovingPlatform : BaseGimmick
{
    public Vector3[] waypoints;
    public float speed = 1f;
    private int currentWaypointIndex = 0;

    private void Update()
    {
        if (isActive && waypoints.Length > 1)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
    }
}

