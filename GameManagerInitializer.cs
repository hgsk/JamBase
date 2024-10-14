using UnityEngine;

public class GameManagerInitializer : MonoBehaviour
{
    public GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager ScriptableObject is not assigned!");
        }
    }

    public void LoadScene(string sceneName)
    {
        gameManager.LoadScene(sceneName);
    }
}
