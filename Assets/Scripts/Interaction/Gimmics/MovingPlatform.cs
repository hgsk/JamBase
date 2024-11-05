using Unity.VisualScripting;
using UnityEngine;
// 移動プラットフォーム
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovingPlatform : BaseGimmick
{
    public Vector3[] waypoints;
    public float speed = 1f;
    private int currentWaypointIndex = 0;

    public string playerTag = "Player";

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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Player Ground");
        // プレイヤーが上から乗った場合
        if (!collision.gameObject.CompareTag(playerTag)) {
            Debug.Log("Player not found");
            return;
        }
        if (!(collision.contacts[0].normal.y < -0.5f)){
            Debug.Log("Player not grounded");
            return;
        }

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
        if (playerRb == null) {
            Debug.Log("Player Rigidbody not found");
            return;
        }
        // プレイヤーにFixedJointを追加
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = collision.gameObject.GetComponent<Rigidbody>(); // プラットフォームのRigidbodyと接続
        joint.enableCollision = false;
        Debug.Log("Player landed on platform");
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            return;
        ReleaseGrounder();
    }

    public void ReleaseGrounder()
    {
        // FixedJointを削除
        FixedJoint joint = gameObject.GetComponent<FixedJoint>();
        if (joint == null)
            return;

        Destroy(joint);
    }

    void OnDrawGizmos()
    {
        if (waypoints.Length > 1)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(waypoints[i], 0.1f);
                if (i < waypoints.Length - 1)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
                }
            }
        }
    }
}

