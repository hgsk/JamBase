using UnityEngine;
using System.Collections;
// 伸縮する足場
public class ExtendablePlatform : BaseGimmick, IInteractableGimmick
{
    public float extendedLength = 5f;
    public float extendSpeed = 1f;
    private Vector3 originalScale;
    private bool isExtended = false;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            StartCoroutine(ExtendCoroutine());
        }
    }

    private IEnumerator ExtendCoroutine()
    {
        Vector3 targetScale = isExtended ? originalScale : new Vector3(extendedLength, originalScale.y, originalScale.z);
        while (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, extendSpeed * Time.deltaTime);
            yield return null;
        }
        isExtended = !isExtended;
    }
}

