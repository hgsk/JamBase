using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// 分身生成装置
public class CloneGenerator : BaseGimmick, IInteractableGimmick
{
    public GameObject clonePrefab;
    public int maxClones = 3;
    private List<GameObject> activeClones = new List<GameObject>();

    public void Interact(GameObject interactor)
    {
        if (isActive && activeClones.Count < maxClones)
        {
            GameObject clone = Instantiate(clonePrefab, interactor.transform.position, interactor.transform.rotation);
            activeClones.Add(clone);
            StartCoroutine(DestroyCloneAfterDelay(clone, 10f));
        }
    }

    private IEnumerator DestroyCloneAfterDelay(GameObject clone, float delay)
    {
        yield return new WaitForSeconds(delay);
        activeClones.Remove(clone);
        Destroy(clone);
    }
}

