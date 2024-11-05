using UnityEngine;
using System.Collections;
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

    private IEnumerator TimerCoroutine()
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

