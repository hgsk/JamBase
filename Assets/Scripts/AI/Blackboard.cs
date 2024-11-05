/*
Blackboardシステムの由来と目的

Blackboard（黒板）システムは、人工知能と認知科学の分野で発展した概念です。
その名前は、複数の専門家が問題解決のために情報を共有する黒板のメタファーに由来しています。

主な目的と利点：
1. 中央データ管理: AIの異なるコンポーネント間でデータを効率的に共有します。
2. モジュール性: 各AIモジュールは独立して動作し、Blackboardを介して通信します。
3. 柔軟性: 新しいAI機能やデータタイプを容易に追加できます。
4. デバッグの容易さ: 中央データストアにより、AIの状態を簡単に観察できます。

ゲーム開発におけるBlackboardシステムは、複雑なAI行動を管理し、
異なるAIコンポーネント（知覚、記憶、意思決定など）間の連携を
スムーズに行うために使用されます。
*/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

// Blackboardに格納されるデータの基本クラス
[Serializable]
public abstract class BlackboardData
{
    public float timestamp;

    protected BlackboardData()
    {
        timestamp = Time.time;
    }
}

// 知覚データを表すクラス
[Serializable]
public class PerceptionData : BlackboardData
{
    public GameObject target;
    public Vector3 lastKnownPosition;

    public PerceptionData(GameObject target, Vector3 position) : base()
    {
        this.target = target;
        this.lastKnownPosition = position;
    }
}

// Blackboard設定を定義するScriptableObject
[CreateAssetMenu(fileName = "New Blackboard Settings", menuName = "AI/Blackboard Settings")]
public class BlackboardSettings : ScriptableObject
{
    [System.Serializable]
    public class BlackboardEntry
    {
        public string key;
        public BlackboardData value;
    }

    public List<BlackboardEntry> initialEntries = new List<BlackboardEntry>();
}

// Blackboardクラス
public class Blackboard
{
    private Dictionary<string, BlackboardData> data = new Dictionary<string, BlackboardData>();

    public void Initialize(BlackboardSettings settings)
    {
        foreach (var entry in settings.initialEntries)
        {
            SetData(entry.key, entry.value);
        }
    }

    public void SetData<T>(string key, T value) where T : BlackboardData
    {
        data[key] = value;
    }

    public T GetData<T>(string key) where T : BlackboardData
    {
        if (data.TryGetValue(key, out BlackboardData value) && value is T)
        {
            return (T)value;
        }
        return null;
    }

    public bool RemoveData(string key)
    {
        return data.Remove(key);
    }

    public void Clear()
    {
        data.Clear();
    }

    public IEnumerable<string> GetAllKeys()
    {
        return data.Keys;
    }
}

// AIManager設定を定義するScriptableObject
[CreateAssetMenu(fileName = "New AI Manager Settings", menuName = "AI/AI Manager Settings")]
public class AIManagerSettings : ScriptableObject
{
    public BlackboardSettings blackboardSettings;
    public float perceptionRadius = 10f;
    public LayerMask targetLayers;
    public float memoryDuration = 30f;
}

// AIManagerコンポーネント
public class AIManager : MonoBehaviour
{
    public AIManagerSettings settings;
    private Blackboard blackboard;
    private PerceptionSystem perceptionSystem;
    private MemorySystem memorySystem;
    private BehaviorSystem behaviorSystem;

    private void Awake()
    {
        blackboard = new Blackboard();
        blackboard.Initialize(settings.blackboardSettings);

        perceptionSystem = gameObject.AddComponent<PerceptionSystem>();
        memorySystem = gameObject.AddComponent<MemorySystem>();
        behaviorSystem = gameObject.AddComponent<BehaviorSystem>();

        perceptionSystem.Initialize(this, settings.perceptionRadius, settings.targetLayers);
        memorySystem.Initialize(this, settings.memoryDuration);
        behaviorSystem.Initialize(this);
    }

    public Blackboard GetBlackboard()
    {
        return blackboard;
    }
}

// 知覚システムコンポーネント
public class PerceptionSystem : MonoBehaviour
{
    private float perceptionRadius;
    private LayerMask targetLayers;
    private Blackboard blackboard;

    public void Initialize(AIManager aiManager, float radius, LayerMask layers)
    {
        blackboard = aiManager.GetBlackboard();
        perceptionRadius = radius;
        targetLayers = layers;
    }

    private void Update()
    {
        PerceiveEnvironment();
    }

    private void PerceiveEnvironment()
    {
        Collider[] perceivedObjects = Physics.OverlapSphere(transform.position, perceptionRadius, targetLayers);

        foreach (Collider col in perceivedObjects)
        {
            GameObject target = col.gameObject;
            Vector3 position = target.transform.position;

            string key = "Perception_" + target.GetInstanceID();
            PerceptionData perceptionData = new PerceptionData(target, position);

            blackboard.SetData(key, perceptionData);
        }
    }
}

// 記憶システムコンポーネント
public class MemorySystem : MonoBehaviour
{
    private float memoryDuration;
    private Blackboard blackboard;

    public void Initialize(AIManager aiManager, float duration)
    {
        blackboard = aiManager.GetBlackboard();
        memoryDuration = duration;
    }

    private void Update()
    {
        UpdateMemories();
    }

    private void UpdateMemories()
    {
        List<string> keysToRemove = new List<string>();

        foreach (var key in blackboard.GetAllKeys())
        {
            if (key.StartsWith("Perception_"))
            {
                PerceptionData perceptionData = blackboard.GetData<PerceptionData>(key);
                if (perceptionData != null)
                {
                    float timeSincePerception = Time.time - perceptionData.timestamp;
                    if (timeSincePerception > memoryDuration)
                    {
                    }
                }
            }
        }
    }
}

// 行動決定システムコンポーネント
public class BehaviorSystem : MonoBehaviour
{
    private Blackboard blackboard;

    public void Initialize(AIManager aiManager)
    {
        blackboard = aiManager.GetBlackboard();
    }

    private void Update()
    {
        DecideBehavior();
    }

    private void DecideBehavior()
    {
        // Blackboardからデータを取得して行動を決定する
        // 例: 最も近い知覚対象に向かって移動する
        GameObject nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (var key in blackboard.GetAllKeys())
        {
            if (key.StartsWith("Perception_"))
            {
                PerceptionData perceptionData = blackboard.GetData<PerceptionData>(key);
                if (perceptionData != null && perceptionData.target != null)
                {
                    float distance = Vector3.Distance(transform.position, perceptionData.lastKnownPosition);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTarget = perceptionData.target;
                    }
                }
            }
        }

        if (nearestTarget != null)
        {
            // 最も近い対象に向かって移動する
            Vector3 direction = (nearestTarget.transform.position - transform.position).normalized;
            transform.position += direction * Time.deltaTime * 5f; // 簡単な移動の例
        }
    }
}