using UnityEngine;
using System.Collections.Generic;

// FSMの状態基本クラス
public abstract class State : ScriptableObject
{
    public BTNode behaviorTree; // この状態に関連付けられたBehavior Tree
    public abstract void Enter(AIDecisionSystem ai);
    public abstract void Update(AIDecisionSystem ai);
    public abstract void Exit(AIDecisionSystem ai);
}

// 複合状態（サブ状態を持つ状態）
public class CompositeState : State
{
    public List<State> subStates;
    public int initialStateIndex;
    private int currentStateIndex;

    public override void Enter(AIDecisionSystem ai)
    {
        currentStateIndex = initialStateIndex;
        subStates[currentStateIndex].Enter(ai);
    }

    public override void Update(AIDecisionSystem ai)
    {
        subStates[currentStateIndex].Update(ai);
        behaviorTree?.Tick(ai);
    }

    public override void Exit(AIDecisionSystem ai)
    {
        subStates[currentStateIndex].Exit(ai);
    }

    public void TransitionTo(int stateIndex, AIDecisionSystem ai)
    {
        if (stateIndex != currentStateIndex && stateIndex < subStates.Count)
        {
            subStates[currentStateIndex].Exit(ai);
            currentStateIndex = stateIndex;
            subStates[currentStateIndex].Enter(ai);
        }
    }
}

// Behavior Treeのノード基本クラス
public abstract class BTNode : ScriptableObject
{
    public enum Status { Success, Failure, Running }
    public abstract Status Tick(AIDecisionSystem ai);
}

// BTコンポジットノード
public abstract class BTComposite : BTNode
{
    public List<BTNode> children;
}

// BTシーケンスノード
[CreateAssetMenu(fileName = "Sequence", menuName = "AI/BT/Sequence")]
public class BTSequence : BTComposite
{
    private int currentChild = 0;

    public override Status Tick(AIDecisionSystem ai)
    {
        while (currentChild < children.Count)
        {
            Status childStatus = children[currentChild].Tick(ai);
            if (childStatus == Status.Failure)
            {
                currentChild = 0;
                return Status.Failure;
            }
            else if (childStatus == Status.Running)
            {
                return Status.Running;
            }
            currentChild++;
        }
        currentChild = 0;
        return Status.Success;
    }
}

// BTセレクターノード
[CreateAssetMenu(fileName = "Selector", menuName = "AI/BT/Selector")]
public class BTSelector : BTComposite
{
    private int currentChild = 0;

    public override Status Tick(AIDecisionSystem ai)
    {
        while (currentChild < children.Count)
        {
            Status childStatus = children[currentChild].Tick(ai);
            if (childStatus == Status.Success)
            {
                currentChild = 0;
                return Status.Success;
            }
            else if (childStatus == Status.Running)
            {
                return Status.Running;
            }
            currentChild++;
        }
        currentChild = 0;
        return Status.Failure;
    }
}

// 具体的な状態の例：巡回状態
[CreateAssetMenu(fileName = "Patrol State", menuName = "AI/States/Patrol")]
public class PatrolState : State
{
    public List<Vector3> patrolPoints;
    private int currentPatrolIndex;

    public override void Enter(AIDecisionSystem ai)
    {
        currentPatrolIndex = 0;
    }

    public override void Update(AIDecisionSystem ai)
    {
        if (ai.transform.position == patrolPoints[currentPatrolIndex])
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }
        ai.transform.position = Vector3.MoveTowards(ai.transform.position, patrolPoints[currentPatrolIndex], Time.deltaTime * 5f);
        
        // 関連するBehavior Treeを実行
        behaviorTree?.Tick(ai);
    }

    public override void Exit(AIDecisionSystem ai) { }
}

// 具体的な状態の例：追跡状態
[CreateAssetMenu(fileName = "Chase State", menuName = "AI/States/Chase")]
public class ChaseState : State
{
    public override void Enter(AIDecisionSystem ai) { }

    public override void Update(AIDecisionSystem ai)
    {
        GameObject target = ai.GetTarget();
        if (target != null)
        {
            ai.transform.position = Vector3.MoveTowards(ai.transform.position, target.transform.position, Time.deltaTime * 7f);
        }
        
        // 関連するBehavior Treeを実行
        behaviorTree?.Tick(ai);
    }

    public override void Exit(AIDecisionSystem ai) { }
}

// BTアクションノードの例：ターゲットをスキャン
[CreateAssetMenu(fileName = "Scan For Target", menuName = "AI/BT/Actions/Scan For Target")]
public class BTScanForTarget : BTNode
{
    public float scanRadius = 10f;
    public LayerMask targetLayer;

    public override Status Tick(AIDecisionSystem ai)
    {
        Collider[] colliders = Physics.OverlapSphere(ai.transform.position, scanRadius, targetLayer);
        if (colliders.Length > 0)
        {
            ai.SetTarget(colliders[0].gameObject);
            return Status.Success;
        }
        return Status.Failure;
    }
}

// BTアクションノードの例：状態を変更
[CreateAssetMenu(fileName = "Change State", menuName = "AI/BT/Actions/Change State")]
public class BTChangeState : BTNode
{
    public State newState;

    public override Status Tick(AIDecisionSystem ai)
    {
        ai.TransitionTo(newState);
        return Status.Success;
    }
}

// AI意思決定システムの設定
[CreateAssetMenu(fileName = "AI Decision Settings", menuName = "AI/AI Decision Settings")]
public class AIDecisionSettings : ScriptableObject
{
    public State initialState;
}

// AI意思決定システム本体
public class AIDecisionSystem : MonoBehaviour
{
    public AIDecisionSettings settings;
    private State currentState;
    private GameObject target;

    private void Start()
    {
        currentState = settings.initialState;
        currentState.Enter(this);
    }

    private void Update()
    {
        currentState.Update(this);
    }

    public void TransitionTo(State newState)
    {
        currentState.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    public GameObject GetTarget()
    {
        return target;
    }
}
// BTConditionノード：条件をチェックする基本クラス
public abstract class BTCondition : BTNode
{
    public abstract bool Check(AIDecisionSystem ai);

    public override Status Tick(AIDecisionSystem ai)
    {
        return Check(ai) ? Status.Success : Status.Failure;
    }
}

// BTActionノード：実際の行動を実行する基本クラス
public abstract class BTAction : BTNode
{
    public abstract Status Execute(AIDecisionSystem ai);

    public override Status Tick(AIDecisionSystem ai)
    {
        return Execute(ai);
    }
}

// BTデコレーターノード：子ノードの結果を修飾する
public abstract class BTDecorator : BTNode
{
    public BTNode child;
}

// BTインバーターノード：子ノードの結果を反転させる
[CreateAssetMenu(fileName = "Inverter", menuName = "AI/BT/Decorators/Inverter")]
public class BTInverter : BTDecorator
{
    public override Status Tick(AIDecisionSystem ai)
    {
        Status childStatus = child.Tick(ai);
        if (childStatus == Status.Success)
            return Status.Failure;
        if (childStatus == Status.Failure)
            return Status.Success;
        return Status.Running;
    }
}

// BTリピーターノード：子ノードを指定回数繰り返す
[CreateAssetMenu(fileName = "Repeater", menuName = "AI/BT/Decorators/Repeater")]
public class BTRepeater : BTDecorator
{
    public int repeatCount = 3;
    private int currentCount = 0;

    public override Status Tick(AIDecisionSystem ai)
    {
        if (currentCount >= repeatCount)
        {
            currentCount = 0;
            return Status.Success;
        }

        Status childStatus = child.Tick(ai);
        if (childStatus == Status.Running)
            return Status.Running;

        currentCount++;
        return Status.Running;
    }
}

// 具体的なBTConditionノードの例：ターゲットが近くにいるかチェック
[CreateAssetMenu(fileName = "Is Target Near", menuName = "AI/BT/Conditions/Is Target Near")]
public class BTIsTargetNear : BTCondition
{
    public float detectionRange = 10f;

    public override bool Check(AIDecisionSystem ai)
    {
        GameObject target = ai.GetTarget();
        return target != null && Vector3.Distance(ai.transform.position, target.transform.position) <= detectionRange;
    }
}

// 具体的なBTActionノードの例：ランダムな位置に移動
[CreateAssetMenu(fileName = "Move Random", menuName = "AI/BT/Actions/Move Random")]
public class BTMoveRandom : BTAction
{
    public float speed = 3f;
    public float range = 10f;
    private Vector3 targetPosition;

    public override Status Execute(AIDecisionSystem ai)
    {
        if (targetPosition == Vector3.zero)
        {
            targetPosition = ai.transform.position + Random.insideUnitSphere * range;
            targetPosition.y = ai.transform.position.y;
        }

        if (Vector3.Distance(ai.transform.position, targetPosition) < 0.1f)
        {
            targetPosition = Vector3.zero;
            return Status.Success;
        }

        ai.transform.position = Vector3.MoveTowards(ai.transform.position, targetPosition, Time.deltaTime * speed);
        return Status.Running;
    }
}

// FSMとBTを組み合わせた具体的な状態の例：警戒状態
[CreateAssetMenu(fileName = "Alert State", menuName = "AI/States/Alert")]
public class AlertState : State
{
    public float alertDuration = 10f;
    private float alertTimer;

    public override void Enter(AIDecisionSystem ai)
    {
        alertTimer = alertDuration;
    }

    public override void Update(AIDecisionSystem ai)
    {
        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0)
        {
            // 警戒時間が終了したら、パトロール状態に戻る例
            ai.TransitionTo(ai.GetState<PatrolState>());
            return;
        }

        // 関連するBehavior Treeを実行
        behaviorTree?.Tick(ai);
    }

    public override void Exit(AIDecisionSystem ai) { }
}

// AIDecisionSystemの拡張メソッド
public static class AIDecisionSystemExtensions
{
    public static T GetState<T>(this AIDecisionSystem ai) where T : State
    {
        // このメソッドの実装は、状態をどのように保持しているかによって異なります
        // 例えば、AIDecisionSettingsにすべての状態のリストがある場合:
        return ai.settings.allStates.Find(s => s is T) as T;
    }
}
