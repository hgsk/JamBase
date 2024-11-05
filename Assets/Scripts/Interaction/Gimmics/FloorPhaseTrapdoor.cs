using UnityEngine;
using System.Collections;
// 床すり抜けトラップ
public class FloorPhaseTrapdoor : BaseGimmick, ITimedGimmick
{
    public float Duration { get; set; } = 3f;
    private Collider floorCollider;

    private void Start()
    {
        floorCollider = GetComponent<Collider>();
    }

    public void StartTimer()
    {
        StartCoroutine(PhaseCoroutine());
    }

    private IEnumerator PhaseCoroutine()
    {
        floorCollider.enabled = false;
        yield return new WaitForSeconds(Duration);
        floorCollider.enabled = true;
    }
}

