using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CharacterActionEvent : UnityEvent<string> { }

public class CharacterEventDispatcher : MonoBehaviour
{
    public CharacterActionEvent OnCharacterAction;

    private DoubleJumpCharacterController controller;
    private Rigidbody rb;
    private bool wasGrounded = true;

    private void Start()
    {
        controller = GetComponent<DoubleJumpCharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (controller.IsGrounded())
        {
            if (!wasGrounded)
            {
                OnCharacterAction.Invoke("Land");
                wasGrounded = true;
            }

            if (rb.velocity.magnitude > 0.1f)
            {
                OnCharacterAction.Invoke("Walk");
            }
            else
            {
                OnCharacterAction.Invoke("Idle");
            }
        }
        else
        {
            if (wasGrounded)
            {
                OnCharacterAction.Invoke("Jump");
                wasGrounded = false;
            }
            else
            {
                OnCharacterAction.Invoke("Fall");
            }
        }
    }
}

public class UnityEventAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void HandleCharacterAction(string action)
    {
        animator.SetTrigger(action);
    }
}

public class UnityEventParticleController : MonoBehaviour
{
    public ParticleSystem jumpEffect;
    public ParticleSystem landEffect;

    public void HandleCharacterAction(string action)
    {
        switch (action)
        {
            case "Jump":
                jumpEffect.Play();
                break;
            case "Land":
                landEffect.Play();
                break;
        }
    }
}

public class UnityEventAudioController : MonoBehaviour
{
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip walkSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void HandleCharacterAction(string action)
    {
        switch (action)
        {
            case "Jump":
                audioSource.PlayOneShot(jumpSound);
                break;
            case "Land":
                audioSource.PlayOneShot(landSound);
                break;
            case "Walk":
                if (!audioSource.isPlaying)
                    audioSource.PlayOneShot(walkSound);
                break;
            case "Idle":
            case "Fall":
                audioSource.Stop();
                break;
        }
    }
}

using UnityEngine.Playables;

public class UnityEventTimelineController : MonoBehaviour
{
    public PlayableDirector jumpTimeline;
    public PlayableDirector landTimeline;

    public void HandleCharacterAction(string action)
    {
        switch (action)
        {
            case "Jump":
                jumpTimeline.Play();
                break;
            case "Land":
                landTimeline.Play();
                break;
        }
    }
}
