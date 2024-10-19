using UnityEngine;
using UnityEngine.InputSystem;

public class EnhancedInputComponent : MonoBehaviour
{
    [Serializable]
    public class EnhancedInputAction
    {
        public string Name;
        public InputActionReference ActionReference;
        public List<InputTrigger> Triggers = new List<InputTrigger>();
    }

    [Serializable]
    public abstract class InputTrigger
    {
        public abstract bool Evaluate(EnhancedInputAction action, InputContext context);
    }

    [Serializable]
    public class InputContext
    {
        public float Time;
        public float Duration;
        public Vector2 Value;
        public bool IsPressed;
    }

    [SerializeField] private List<EnhancedInputAction> actions = new List<EnhancedInputAction>();

    private Dictionary<string, List<Action<InputContext>>> callbacks = new Dictionary<string, List<Action<InputContext>>>();

    private void Awake()
    {
        SetupInputListeners();
    }

    private void SetupInputListeners()
    {
        foreach (var action in actions)
        {
            action.ActionReference.action.performed += ctx => OnInputPerformed(action, ctx);
            action.ActionReference.action.canceled += ctx => OnInputCanceled(action, ctx);
        }
    }

    private void OnInputPerformed(EnhancedInputAction action, InputAction.CallbackContext ctx)
    {
        var inputContext = new InputContext
        {
            Time = Time.time,
            Duration = (float) ctx.duration,
            Value = ctx.ReadValue<Vector2>(),
            IsPressed = ctx.performed
        };

        EvaluateTriggers(action, inputContext);
    }

    private void OnInputCanceled(EnhancedInputAction action, InputAction.CallbackContext ctx)
    {
        var inputContext = new InputContext
        {
            Time = Time.time,
            Duration = (float) ctx.duration,
            Value = Vector2.zero,
            IsPressed = false
        };

        EvaluateTriggers(action, inputContext);
    }

    private void EvaluateTriggers(EnhancedInputAction action, InputContext context)
    {
        foreach (var trigger in action.Triggers)
        {
            if (trigger.Evaluate(action, context))
            {
                InvokeCallbacks(action.Name, context);
            }
        }
    }

    private void InvokeCallbacks(string actionName, InputContext context)
    {
        if (callbacks.TryGetValue(actionName, out var actionCallbacks))
        {
            foreach (var callback in actionCallbacks)
            {
                callback.Invoke(context);
            }
        }
    }

    public void BindAction(string actionName, Action<InputContext> callback)
    {
        if (!callbacks.ContainsKey(actionName))
        {
            callbacks[actionName] = new List<Action<InputContext>>();
        }
        callbacks[actionName].Add(callback);
    }

    public void UnbindAction(string actionName, Action<InputContext> callback)
    {
        if (callbacks.ContainsKey(actionName))
        {
            callbacks[actionName].Remove(callback);
        }
    }

    internal void SetupActions(List<EnhancedInputAction> enhancedInputActions)
    {
        throw new NotImplementedException();
    }

    internal InputActionReference GetActionReference(string inputActionReference)
    {
        throw new NotImplementedException();
    }

    internal void ClearAllBindings()
    {
        throw new NotImplementedException();
    }
}

// Example Triggers
public class TapTrigger : EnhancedInputComponent.InputTrigger
{
    public override bool Evaluate(EnhancedInputComponent.EnhancedInputAction action, EnhancedInputComponent.InputContext context)
    {
        return context.IsPressed && context.Duration < 0.2f;
    }
}

public class HoldTrigger : EnhancedInputComponent.InputTrigger
{
    public float HoldDuration = 0.5f;

    public override bool Evaluate(EnhancedInputComponent.EnhancedInputAction action, EnhancedInputComponent.InputContext context)
    {
        return context.IsPressed && context.Duration >= HoldDuration;
    }
}

public class MultiTapTrigger : EnhancedInputComponent.InputTrigger
{
    public int TapCount = 2;
    public float TapWindow = 0.3f;

    private List<float> tapTimes = new List<float>();

    public override bool Evaluate(EnhancedInputComponent.EnhancedInputAction action, EnhancedInputComponent.InputContext context)
    {
        if (context.IsPressed)
        {
            tapTimes.Add(context.Time);
            if (tapTimes.Count > TapCount)
            {
                tapTimes.RemoveAt(0);
            }

            if (tapTimes.Count == TapCount && 
                (tapTimes[TapCount - 1] - tapTimes[0]) <= TapWindow)
            {
                tapTimes.Clear();
                return true;
            }
        }

        // Clean up old taps
        tapTimes.RemoveAll(t => context.Time - t > TapWindow);

        return false;
    }
}

// Example Composite Trigger
public class ComboTrigger : EnhancedInputComponent.InputTrigger
{
    public List<EnhancedInputComponent.InputTrigger> Sequence = new List<EnhancedInputComponent.InputTrigger>();
    public float ComboWindow = 1.0f;

    private List<float> triggerTimes = new List<float>();
    private int currentIndex = 0;

    public override bool Evaluate(EnhancedInputComponent.EnhancedInputAction action, EnhancedInputComponent.InputContext context)
    {
        if (currentIndex < Sequence.Count && Sequence[currentIndex].Evaluate(action, context))
        {
            triggerTimes.Add(context.Time);
            currentIndex++;

            if (currentIndex == Sequence.Count)
            {
                if (triggerTimes[triggerTimes.Count - 1] - triggerTimes[0] <= ComboWindow)
                {
                    ResetCombo();
                    return true;
                }
                else
                {
                    ResetCombo();
                }
            }
        }

        // Clean up old triggers
        while (triggerTimes.Count > 0 && context.Time - triggerTimes[0] > ComboWindow)
        {
            triggerTimes.RemoveAt(0);
            currentIndex = 0;
        }

        return false;
    }

    private void ResetCombo()
    {
        triggerTimes.Clear();
        currentIndex = 0;
    }
}
