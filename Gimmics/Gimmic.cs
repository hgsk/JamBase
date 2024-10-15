using UnityEngine;

public interface IGimmick
{
    void Activate();
    void Deactivate();
    bool IsActive { get; }
}

public interface IInteractableGimmick : IGimmick
{
    void Interact(GameObject interactor);
}

public interface ITimedGimmick : IGimmick
{
    float Duration { get; set; }
    void StartTimer();
}

public interface IAreaGimmick : IGimmick
{
    bool IsInArea(Vector3 position);
}

// ギミック共通実装
public abstract class BaseGimmick : MonoBehaviour, IGimmick
{
    protected bool isActive;

    public virtual void Activate()
    {
        isActive = true;
    }

    public virtual void Deactivate()
    {
        isActive = false;
    }

    public bool IsActive => isActive;
}

// ジャンプパッド
public class JumpPad : BaseGimmick, IInteractableGimmick
{
    public float jumpForce = 10f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}

// 滑る床
public class SlipperyFloor : BaseGimmick, IAreaGimmick
{
    public float frictionCoefficient = 0.1f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isActive && collision.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.AddForce(velocity.normalized * frictionCoefficient, ForceMode.Acceleration);
        }
    }
}

// テレポーター
public class Teleporter : BaseGimmick, IInteractableGimmick
{
    public Transform destination;

    public void Interact(GameObject interactor)
    {
        if (isActive && destination != null)
        {
            interactor.transform.position = destination.position;
        }
    }
}

// 重力反転装置
public class GravityReverser : BaseGimmick, IAreaGimmick, ITimedGimmick
{
    public float Duration { get; set; } = 5f;
    public Collider areaCollider;
    private float timeLeft;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    public void StartTimer()
    {
        timeLeft = Duration;
        StartCoroutine(TimerCoroutine());
    }

    private System.Collections.IEnumerator TimerCoroutine()
    {
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        Deactivate();
    }

    public override void Activate()
    {
        base.Activate();
        Physics.gravity = new Vector3(0, 9.81f, 0);
        StartTimer();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }
}
