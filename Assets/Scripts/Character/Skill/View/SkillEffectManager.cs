using UnityEngine;
using System.Collections.Generic;

// ScriptableObject for skill visual and audio data
[CreateAssetMenu(fileName = "New Skill Effect Data", menuName = "Skill System/Skill Effect Data")]
public class SkillEffectDataSO : ScriptableObject
{
    public string skillName;
    public string animationTriggerName;
    public GameObject particleEffectPrefab;
    public AudioClip soundEffect;
    public float effectDuration = 1f;
}

// Manager for character skill visual and audio effects
public class CharacterSkillEffectManager : MonoBehaviour
{
    public Transform effectSpawnPoint;

    private Animator animator;
    private AudioSource audioSource;
    private Dictionary<string, SkillEffectDataSO> effectData = new Dictionary<string, SkillEffectDataSO>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        LoadEffectData();
    }

    private void LoadEffectData()
    {
        SkillEffectDataSO[] allEffectData = Resources.LoadAll<SkillEffectDataSO>("SkillEffectData");
        foreach (var data in allEffectData)
        {
            effectData[data.skillName] = data;
        }
    }

    public void PlaySkillEffect(string skillName)
    {
        if (effectData.TryGetValue(skillName, out SkillEffectDataSO data))
        {
            // Play animation
            if (!string.IsNullOrEmpty(data.animationTriggerName) && animator != null)
            {
                animator.SetTrigger(data.animationTriggerName);
            }

            // Spawn particle effect
            if (data.particleEffectPrefab != null)
            {
                GameObject effect = Instantiate(data.particleEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
                Destroy(effect, data.effectDuration);
            }

            // Play sound effect
            if (data.soundEffect != null && audioSource != null)
            {
                audioSource.PlayOneShot(data.soundEffect);
            }
        }
    }
}

// Observer interface for skill usage
public interface ISkillObserver
{
    void OnSkillUsed(string skillName);
}

// Extension of SkillManager to include observer pattern
public class ObservableSkillManager : SkillManager
{
    private List<ISkillObserver> observers = new List<ISkillObserver>();

    public void AddObserver(ISkillObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(ISkillObserver observer)
    {
        observers.Remove(observer);
    }

    protected void NotifySkillUsed(string skillName)
    {
        foreach (var observer in observers)
        {
            observer.OnSkillUsed(skillName);
        }
    }

    // Override the UseSkill method to include notification
    public override void UseSkill(string skillName)
    {
        base.UseSkill(skillName);
        NotifySkillUsed(skillName);
    }
}

// Skill effect controller that observes skill usage
public class SkillEffectController : MonoBehaviour, ISkillObserver
{
    private CharacterSkillEffectManager effectManager;
    private ObservableSkillManager skillManager;

    private void Start()
    {
        effectManager = GetComponent<CharacterSkillEffectManager>();
        skillManager = GetComponent<ObservableSkillManager>();

        if (skillManager != null)
        {
            skillManager.AddObserver(this);
        }
    }

    public void OnSkillUsed(string skillName)
    {
        effectManager.PlaySkillEffect(skillName);
    }

    private void OnDestroy()
    {
        if (skillManager != null)
        {
            skillManager.RemoveObserver(this);
        }
    }
}

// Example of how to set up the system in a MonoBehaviour
public class CharacterSkillViewSetup : MonoBehaviour
{
    private void Start()
    {
        // Ensure all necessary components are attached
        if (GetComponent<CharacterSkillEffectManager>() == null)
        {
            gameObject.AddComponent<CharacterSkillEffectManager>();
        }

        if (GetComponent<ObservableSkillManager>() == null)
        {
            gameObject.AddComponent<ObservableSkillManager>();
        }

        if (GetComponent<SkillEffectController>() == null)
        {
            gameObject.AddComponent<SkillEffectController>();
        }
    }
}
