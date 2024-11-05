using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

// 入力アクション定義のScriptableObject
[CreateAssetMenu(fileName = "New Input Action", menuName = "Input System/Input Action")]
public class InputActionSO : ScriptableObject
{
    public string actionName;
    public string inputActionReference; // EnhancedInputComponentのアクション参照の名前
    public List<InputTriggerSO> triggers = new List<InputTriggerSO>();
}

// 入力トリガー定義の基底ScriptableObject
public abstract class InputTriggerSO : ScriptableObject
{
    public abstract bool Evaluate(EnhancedInputComponent.InputContext context);
}

// 具体的なトリガータイプのScriptableObject
[CreateAssetMenu(fileName = "New Tap Trigger", menuName = "Input System/Triggers/Tap Trigger")]
public class TapTriggerSO : InputTriggerSO
{
    public override bool Evaluate(EnhancedInputComponent.InputContext context)
    {
        return context.IsPressed && context.Duration < 0.2f;
    }
}

[CreateAssetMenu(fileName = "New Hold Trigger", menuName = "Input System/Triggers/Hold Trigger")]
public class HoldTriggerSO : InputTriggerSO
{
    public float holdTime = 0.5f;

    public override bool Evaluate(EnhancedInputComponent.InputContext context)
    {
        return context.IsPressed && context.Duration >= holdTime;
    }
}

[CreateAssetMenu(fileName = "New Double Tap Trigger", menuName = "Input System/Triggers/Double Tap Trigger")]
public class DoubleTapTriggerSO : InputTriggerSO
{
    public float doubleTapWindow = 0.3f;
    private float lastTapTime;
    private int tapCount;

    public override bool Evaluate(EnhancedInputComponent.InputContext context)
    {
        if (context.IsPressed)
        {
            if (Time.time - lastTapTime <= doubleTapWindow)
            {
                tapCount++;
                if (tapCount == 2)
                {
                    tapCount = 0;
                    return true;
                }
            }
            else
            {
                tapCount = 1;
            }
            lastTapTime = Time.time;
        }
        return false;
    }
}

// 入力戦略のScriptableObject
[CreateAssetMenu(fileName = "New Input Strategy", menuName = "Input System/Input Strategy")]
public class InputStrategySO : ScriptableObject
{
    public List<InputActionSO> actions = new List<InputActionSO>();
}

// 入力ハンドラー
public class CharacterInputHandler : MonoBehaviour
{
    public AdvancedPhysicsBasedCharacterController.InputEvents inputEvents;
    public InputStrategySO currentStrategy;
    private EnhancedInputComponent enhancedInput;

    private void Start()
    {
        enhancedInput = GetComponent<EnhancedInputComponent>();
        if (currentStrategy != null)
        {
            SetupInputActions(currentStrategy);
        }
    }

    public void SetInputStrategy(InputStrategySO newStrategy)
    {
        currentStrategy = newStrategy;
        SetupInputActions(newStrategy);
    }

    private void SetupInputActions(InputStrategySO strategy)
    {
        enhancedInput.ClearAllBindings();
        foreach (var action in strategy.actions)
        {
            var inputAction = new EnhancedInputComponent.EnhancedInputAction
            {
                Name = action.actionName,
                ActionReference = enhancedInput.GetActionReference(action.inputActionReference),
                Triggers = new List<EnhancedInputComponent.InputTrigger>(
                    action.triggers.ConvertAll(t => new InputTriggerAdapter(t))
                )
            };
            enhancedInput.BindAction(action.actionName, context => OnInputAction(action.actionName, context));
        }
        enhancedInput.SetupActions(strategy.actions.ConvertAll(a => new EnhancedInputComponent.EnhancedInputAction
        {
            Name = a.actionName,
            ActionReference = enhancedInput.GetActionReference(a.inputActionReference),
            Triggers = new List<EnhancedInputComponent.InputTrigger>(
                a.triggers.ConvertAll(t => new InputTriggerAdapter(t))
            )
        }));
    }

    private void OnInputAction(string actionName, EnhancedInputComponent.InputContext context)
    {
        if (actionName == "Move")
        {
            inputEvents.OnMove.Invoke(context.Value);
        }
        else
        {
            inputEvents.OnCustomAction.Invoke(actionName, context);
        }
    }
}

// InputTriggerSOをEnhancedInputComponent.InputTriggerに適応させるアダプター
public class InputTriggerAdapter : EnhancedInputComponent.InputTrigger
{
    private InputTriggerSO triggerSO;

    public InputTriggerAdapter(InputTriggerSO triggerSO)
    {
        this.triggerSO = triggerSO;
    }

    public override bool Evaluate(EnhancedInputComponent.EnhancedInputAction action, EnhancedInputComponent.InputContext context)
    {
        return triggerSO.Evaluate(context);
    }
}
