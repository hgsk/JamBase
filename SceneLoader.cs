using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public SceneManagerSO sceneManagerSO;

    private void Awake()
    {
        if (sceneManagerSO == null)
        {
            Debug.LogError("SceneManagerSO is not assigned!");
        }
    }

    public void LoadScene(string sceneName)
    {
        sceneManagerSO.LoadScene(sceneName);
    }
}
