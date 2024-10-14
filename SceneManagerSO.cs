using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        
        foreach (var transition in sceneTransitions)
        {
            if (transition.sceneName == sceneName)
            {
                transition.onSceneLoad.Invoke();
                break;
            }
        }
    }
}
