/*
1. くっつけたいオブジェクトにこのスクリプトをアタッチします。
2. 同じオブジェクトにRigidbodyコンポーネントがあることを確認します。
3. Inspectorで`targetTag`と`attachForce`を適切に設定します。
4. くっつく対象となるオブジェクトに、スクリプトで指定したタグを設定し、Rigidbodyコンポーネントを追加します。
*/
using UnityEngine;

public class AttachObject : MonoBehaviour
{
    public string targetTag = "Attachable"; // くっつく対象のタグ
    public bool isAttached = false; // くっついているかどうか
    public float attachForce = 10f; // くっつく力

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbodyコンポーネントが必要です");
            enabled = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isAttached && collision.gameObject.CompareTag(targetTag))
        {
            Attach(collision.transform);
        }
    }

    private void Attach(Transform target)
    {
        isAttached = true;
        rb.isKinematic = true; // 物理演算を無効化

        // FixedJointを追加してオブジェクトをくっつける
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = target.GetComponent<Rigidbody>();
        joint.breakForce = attachForce;
        joint.breakTorque = attachForce;

        // ジョイントが壊れた時のイベントを登録
        joint.breakForce = attachForce;
        joint.breakTorque = attachForce;
        Invoke("CheckJointBreak", 0.1f); // 少し遅延を入れてジョイントの破壊をチェック
    }

    private void CheckJointBreak()
    {
        if (GetComponent<FixedJoint>() == null)
        {
            Detach();
        }
        else
        {
            Invoke("CheckJointBreak", 0.1f); // 継続的にチェック
        }
    }

    public void Detach()
    {
        if (isAttached)
        {
            isAttached = false;
            rb.isKinematic = false; // 物理演算を再有効化

            FixedJoint joint = GetComponent<FixedJoint>();
            if (joint != null)
            {
                Destroy(joint);
            }
        }
    }
}
