using UnityEngine;
using UnityEngine.Events;

// トリガーイベント用
[System.Serializable]
public class TriggerEvent : UnityEvent<Collider> { }

// コリジョンイベント用
[System.Serializable]
public class CollisionEvent : UnityEvent<Collision> { }

public class PhysicsEventHandler : MonoBehaviour
{
    [SerializeField]
    private TriggerEvent onTriggerEntered = new TriggerEvent();
    
    [SerializeField]
    private CollisionEvent onCollisionEntered = new CollisionEvent();

    [Header("Filtering Settings")]
    [SerializeField]
    private LayerMask targetLayerMask = -1;
    [SerializeField]
    private string[] targetTags;
    [SerializeField]
    private bool requireRigidbody = false;

    // プロパティ
    public TriggerEvent OnTriggerEntered => onTriggerEntered;
    public CollisionEvent OnCollisionEntered => onCollisionEntered;

    private bool ValidateTarget(GameObject target)
    {
        // レイヤーマスクチェック
        if ((targetLayerMask.value & (1 << target.layer)) == 0)
            return false;

        // タグチェック
        if (targetTags != null && targetTags.Length > 0)
        {
            bool hasMatchingTag = false;
            foreach (string tag in targetTags)
            {
                if (target.CompareTag(tag))
                {
                    hasMatchingTag = true;
                    break;
                }
            }
            if (!hasMatchingTag)
                return false;
        }

        // Rigidbodyチェック
        if (requireRigidbody && target.GetComponent<Rigidbody>() == null)
            return false;

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ValidateTarget(other.gameObject))
        {
            onTriggerEntered.Invoke(other);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ValidateTarget(collision.gameObject))
        {
            onCollisionEntered.Invoke(collision);
        }
    }

    // デバッグ用の設定表示
    private void OnValidate()
    {
        if (requireRigidbody)
        {
            Debug.Log("Note: Rigidbody check is enabled. Make sure target objects have Rigidbody component.");
        }
    }
}