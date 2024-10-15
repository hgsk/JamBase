using UnityEngine;
using UnityEngine.Playables;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    public PlayableDirector inTransition;
    public PlayableDirector outTransition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayInTransition()
    {
        if (inTransition != null)
        {
            inTransition.Play();
        }
    }

    public void PlayOutTransition()
    {
        if (outTransition != null)
        {
            outTransition.Play();
        }
    }
}
