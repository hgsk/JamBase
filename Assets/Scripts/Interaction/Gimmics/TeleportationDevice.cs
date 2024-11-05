using UnityEngine;
using Random = UnityEngine.Random;
// ランダム瞬間移動装置
public class TeleportationDevice : BaseGimmick, IInteractableGimmick
{
    public Transform[] teleportPoints;

    public void Interact(GameObject interactor)
    {
        if (isActive && teleportPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, teleportPoints.Length);
            interactor.transform.position = teleportPoints[randomIndex].position;
        }
    }
}

