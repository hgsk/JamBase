using UnityEngine;
using System.Collections;
// サイズ変更ポーション
public class SizeChangePotion : BaseGimmick, IInteractableGimmick
{
    public float sizeMultiplier = 2f;
    public float duration = 10f;

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            StartCoroutine(ChangeSizeCoroutine(interactor));
        }
    }

    private IEnumerator ChangeSizeCoroutine(GameObject target)
    {
        Vector3 originalScale = target.transform.localScale;
        target.transform.localScale *= sizeMultiplier;
        yield return new WaitForSeconds(duration);
        target.transform.localScale = originalScale;
    }
}

