using UnityEngine;
// 水中呼吸エリア
public class UnderwaterBreathingZone : BaseGimmick, IAreaGimmick
{
    public float oxygenReplenishRate = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<IOxygenDependent>(out var oxygenDependent))
        {
            oxygenDependent.ReplenishOxygen(oxygenReplenishRate * Time.deltaTime);
        }
    }
}

