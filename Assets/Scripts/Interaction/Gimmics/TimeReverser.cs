using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// 時間逆行装置
public class TimeReverser : BaseGimmick, ITimedGimmick
{
    public float Duration { get; set; } = 5f;
    private List<IReversible> reversibleObjects = new List<IReversible>();

    public void StartTimer()
    {
        StartCoroutine(ReverseTimeCoroutine());
    }

    private IEnumerator ReverseTimeCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < Duration)
        {
            foreach (var reversible in reversibleObjects)
            {
                reversible.ReverseTime(Time.deltaTime);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IReversible>(out var reversible))
        {
            reversibleObjects.Add(reversible);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IReversible>(out var reversible))
        {
            reversibleObjects.Remove(reversible);
        }
    }
}

