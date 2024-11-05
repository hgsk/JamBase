using UnityEngine;

internal interface IDamageDealer
{
    float DamageAmount { get; set; }
}

public interface IGimmick
{
    void Activate();
    void Deactivate();
    bool IsActive { get; }
}

public interface IInteractableGimmick : IGimmick
{
    void Interact(GameObject interactor);
}

public interface ITimedGimmick : IGimmick
{
    float Duration { get; set; }
    void StartTimer();
}

public interface IAreaGimmick : IGimmick
{
    bool IsInArea(Vector3 position);
}

// ギミック共通実装
public abstract class BaseGimmick : MonoBehaviour, IGimmick
{
    protected bool isActive;

    public virtual void Activate()
    {
        isActive = true;
    }

    public virtual void Deactivate()
    {
        isActive = false;
    }

    public bool IsActive => isActive;
}

internal interface IElemental
{
    void ChangeElement(ElementChangeGate.Element newElement);
}

internal interface IOxygenDependent
{
    void ReplenishOxygen(float v);
}

internal interface IReversible
{
    void ReverseTime(float deltaTime);
}

internal interface IShadowMergeable
{
    void DisableShadowMerge();
    void EnableShadowMerge();
}

