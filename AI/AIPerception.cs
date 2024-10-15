/*
1. 必要な知覚タイプ（VisionSense、HearingSense等）のScriptableObjectをプロジェクト内で作成します。
2. AIPerceptionのScriptableObjectを作成し、使用する知覚タイプを設定します。
3. AIエージェントのゲームオブジェクトにAIPerceptionComponentをアタッチし、作成したAIPerceptionをアサインします。
4. AIの行動を制御するスクリプト内で、`AIPerceptionComponent.GetPerceivedTargets()`メソッドを呼び出して、知覚された対象を取得し、それに基づいて行動を決定します。
*/

using UnityEngine;
using System.Collections.Generic;

// AIの知覚設定を定義するScriptableObject
[CreateAssetMenu(fileName = "New AI Perception", menuName = "AI/Perception")]
public class AIPerception : ScriptableObject
{
    public List<Sense> senses = new List<Sense>();
}

// 知覚の基本クラス
public abstract class Sense : ScriptableObject
{
    public float updateInterval = 0.5f; // 知覚の更新間隔
    public abstract void Perceive(AIPerceptionComponent perceptionComponent);
}

// 視覚の実装
[CreateAssetMenu(fileName = "New Vision Sense", menuName = "AI/Senses/Vision")]
public class VisionSense : Sense
{
    public float viewRadius = 10f;
    public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public override void Perceive(AIPerceptionComponent perceptionComponent)
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(perceptionComponent.transform.position, viewRadius, targetMask);

        foreach (Collider target in targetsInViewRadius)
        {
            Vector3 dirToTarget = (target.transform.position - perceptionComponent.transform.position).normalized;
            if (Vector3.Angle(perceptionComponent.transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(perceptionComponent.transform.position, target.transform.position);
                if (!Physics.Raycast(perceptionComponent.transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    perceptionComponent.AddPerceivedTarget(target.gameObject);
                }
            }
        }
    }
}

// 聴覚の実装
[CreateAssetMenu(fileName = "New Hearing Sense", menuName = "AI/Senses/Hearing")]
public class HearingSense : Sense
{
    public float hearingRadius = 15f;
    public LayerMask soundSourceMask;

    public override void Perceive(AIPerceptionComponent perceptionComponent)
    {
        Collider[] soundSourcesInRadius = Physics.OverlapSphere(perceptionComponent.transform.position, hearingRadius, soundSourceMask);

        foreach (Collider soundSource in soundSourcesInRadius)
        {
            // 本来なら音の大きさや障害物なども考慮すべきですが、簡略化のため単純に範囲内にあるものを全て知覚します
            perceptionComponent.AddPerceivedTarget(soundSource.gameObject);
        }
    }
}

// AIPerceptionComponentをアタッチするゲームオブジェクト用のコンポーネント
public class AIPerceptionComponent : MonoBehaviour
{
    public AIPerception perception;
    private Dictionary<Sense, float> lastUpdateTime = new Dictionary<Sense, float>();
    private HashSet<GameObject> perceivedTargets = new HashSet<GameObject>();

    private void Update()
    {
        if (perception == null) return;

        foreach (Sense sense in perception.senses)
        {
            if (!lastUpdateTime.ContainsKey(sense) || Time.time - lastUpdateTime[sense] >= sense.updateInterval)
            {
                sense.Perceive(this);
                lastUpdateTime[sense] = Time.time;
            }
        }
    }

    public void AddPerceivedTarget(GameObject target)
    {
        perceivedTargets.Add(target);
    }

    public HashSet<GameObject> GetPerceivedTargets()
    {
        return new HashSet<GameObject>(perceivedTargets);
    }

    public void ClearPerceivedTargets()
    {
        perceivedTargets.Clear();
    }

    // デバッグ用の視覚化メソッド
    private void OnDrawGizmos()
    {
        if (perception == null) return;

        foreach (Sense sense in perception.senses)
        {
            if (sense is VisionSense)
            {
                VisionSense vision = (VisionSense)sense;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, vision.viewRadius);

                Vector3 viewAngleA = DirFromAngle(-vision.viewAngle / 2, false);
                Vector3 viewAngleB = DirFromAngle(vision.viewAngle / 2, false);

                Gizmos.DrawLine(transform.position, transform.position + viewAngleA * vision.viewRadius);
                Gizmos.DrawLine(transform.position, transform.position + viewAngleB * vision.viewRadius);
            }
            else if (sense is HearingSense)
            {
                HearingSense hearing = (HearingSense)sense;
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, hearing.hearingRadius);
            }
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
