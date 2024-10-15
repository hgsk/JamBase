using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

[CreateAssetMenu(fileName = "SceneManagerSO", menuName = "ScriptableObjects/SceneManagerSO")]
public class SceneManagerSO : ScriptableObject
{
    [System.Serializable]
    public class SceneTransition
    {
        public string sceneName;
        public UnityEvent onSceneLoad;
    }

    public SceneTransition[] sceneTransitions;
    public float transitionDuration = 1f;

    public void LoadScene(string sceneName)
    {
        SceneTransitionManager.Instance.PlayOutTransition();
        CoroutineRunner.Instance.StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(sceneName);
        
        foreach (var transition in sceneTransitions)
        {
            if (transition.sceneName == sceneName)
            {
                transition.onSceneLoad.Invoke();
                break;
            }
        }

        SceneTransitionManager.Instance.PlayInTransition();
    }
}
