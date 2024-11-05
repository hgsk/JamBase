using UnityEngine;
using System.Collections;
// 影潜り能力付与装置
public class ShadowMergeDevice : BaseGimmick, IInteractableGimmick
{
    public float duration = 10f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<IShadowMergeable>(out var shadowMergeable))
        {
            StartCoroutine(ShadowMergeCoroutine(shadowMergeable));
        }
    }

    private IEnumerator ShadowMergeCoroutine(IShadowMergeable shadowMergeable)
    {
        shadowMergeable.EnableShadowMerge();
        yield return new WaitForSeconds(duration);
        shadowMergeable.DisableShadowMerge();
    }
}

