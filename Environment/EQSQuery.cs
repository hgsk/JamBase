using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// 環境クエリの設定を定義する
[CreateAssetMenu(fileName = "New EQS Query", menuName = "EQS/Query")]
public class EQSQuery : ScriptableObject
{
    public QueryGenerator generator;
    public List<QueryTest> tests;
    public QueryResultSelector resultSelector;
}

// クエリ位置を生成する
public abstract class QueryGenerator : ScriptableObject
{
    public abstract List<Vector3> GenerateQueryPositions(Vector3 center, float radius);
}

// グリッド状にクエリ位置を生成する
[CreateAssetMenu(fileName = "New Grid Generator", menuName = "EQS/Generators/Grid")]
public class GridGenerator : QueryGenerator
{
    public int gridSize = 5;
    public float spacing = 1f;

    public override List<Vector3> GenerateQueryPositions(Vector3 center, float radius)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int x = -gridSize/2; x <= gridSize/2; x++)
        {
            for (int z = -gridSize/2; z <= gridSize/2; z++)
            {
                Vector3 pos = center + new Vector3(x * spacing, 0, z * spacing);
                if (Vector3.Distance(center, pos) <= radius)
                {
                    positions.Add(pos);
                }
            }
        }
        return positions;
    }
}

// クエリテスト
public abstract class QueryTest : ScriptableObject
{
    public float weight = 1f;
    public abstract float RunTest(Vector3 queryPosition);
}

// 距離に基づくテスト
[CreateAssetMenu(fileName = "New Distance Test", menuName = "EQS/Tests/Distance")]
public class DistanceTest : QueryTest
{
    public Vector3 targetPosition;

    public override float RunTest(Vector3 queryPosition)
    {
        float distance = Vector3.Distance(queryPosition, targetPosition);
        return 1f / (1f + distance); // 距離が近いほど高スコア
    }
}

// NavMeshに基づく到達可能性テスト
[CreateAssetMenu(fileName = "New Reachability Test", menuName = "EQS/Tests/Reachability")]
public class ReachabilityTest : QueryTest
{
    public override float RunTest(Vector3 queryPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(queryPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            return 1f; // NavMesh上にある場合は最高スコア
        }
        return 0f; // NavMesh上にない場合は最低スコア
    }
}

// クエリ結果の選択方法を定義する
public abstract class QueryResultSelector : ScriptableObject
{
    public abstract Vector3 SelectResult(List<Vector3> positions, List<float> scores);
}

// 最高スコアの結果を選択するセレクター
[CreateAssetMenu(fileName = "New Best Score Selector", menuName = "EQS/Selectors/Best Score")]
public class BestScoreSelector : QueryResultSelector
{
    public override Vector3 SelectResult(List<Vector3> positions, List<float> scores)
    {
        int bestIndex = 0;
        float bestScore = float.MinValue;
        for (int i = 0; i < scores.Count; i++)
        {
            if (scores[i] > bestScore)
            {
                bestScore = scores[i];
                bestIndex = i;
            }
        }
        return positions[bestIndex];
    }
}

// EQSシステム
public class EnvironmentQuerySystem : MonoBehaviour
{
    public EQSQuery query;
    public float queryRadius = 10f;

    public Vector3 RunQuery()
    {
        Vector3 center = transform.position;
        List<Vector3> queryPositions = query.generator.GenerateQueryPositions(center, queryRadius);
        List<float> scores = new List<float>(queryPositions.Count);

        foreach (Vector3 position in queryPositions)
        {
            float totalScore = 0f;
            foreach (QueryTest test in query.tests)
            {
                totalScore += test.RunTest(position) * test.weight;
            }
            scores.Add(totalScore);
        }

        return query.resultSelector.SelectResult(queryPositions, scores);
    }

    // デバッグ用の視覚化メソッド
    private void OnDrawGizmos()
    {
        if (query == null) return;

        Vector3 center = transform.position;
        List<Vector3> queryPositions = query.generator.GenerateQueryPositions(center, queryRadius);

        Gizmos.color = Color.yellow;
        foreach (Vector3 position in queryPositions)
        {
            Gizmos.DrawSphere(position, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, queryRadius);
    }
}
