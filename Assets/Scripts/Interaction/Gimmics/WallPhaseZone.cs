using UnityEngine;
// 壁すり抜けゾーン
public class WallPhaseZone : BaseGimmick, IAreaGimmick
{
    public LayerMask wallLayers;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive)
        {
            Physics.IgnoreLayerCollision(other.gameObject.layer, wallLayers, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Physics.IgnoreLayerCollision(other.gameObject.layer, wallLayers, false);
    }
}

