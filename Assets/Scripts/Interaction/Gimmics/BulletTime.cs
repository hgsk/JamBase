using UnityEngine;
using System.Collections;
// 一時スローモーション（バレットタイム）
public class BulletTime : BaseGimmick, ITimedGimmick
{
    public float slowdownFactor = 0.2f;
    public float Duration { get; set; } = 5f;

    public void StartTimer()
    {
        StartCoroutine(BulletTimeCoroutine());
    }

    private IEnumerator BulletTimeCoroutine()
    {
        Time.timeScale = slowdownFactor;
        yield return new WaitForSecondsRealtime(Duration);
        Time.timeScale = 1f;
    }
}

