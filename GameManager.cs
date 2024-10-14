using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameManager", menuName = "ScriptableObjects/GameManager")]
public class GameManager : ScriptableObject
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
