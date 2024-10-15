using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Sound Effect", menuName = "Audio/Sound Effect")]
public class SoundEffectSO : ScriptableObject
{
    public string effectName;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1f;
}

// ScriptableObject for music track
[CreateAssetMenu(fileName = "New Music Track", menuName = "Audio/Music Track")]
public class MusicTrackSO : ScriptableObject
{
    public string trackName;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
}

// ScriptableObject for entire sound configuration
[CreateAssetMenu(fileName = "New Sound Configuration", menuName = "Audio/Sound Configuration")]
public class SoundConfigurationSO : ScriptableObject
{
    public List<SoundEffectSO> soundEffects;
    public List<MusicTrackSO> musicTracks;
}

// Updated SkillEffect to include sound effect name
[CreateAssetMenu(fileName = "New Skill Effect", menuName = "Skill System/Skill Effect")]
public class SkillEffect : ScriptableObject
{
    public string skillName;
    public string animationTrigger;
    public GameObject particleEffect;
    public string soundEffectName; // Name of the sound effect to play
    public float effectDuration = 1f;
}

// Updated SoundManager to implement ISkillObserver
public class SoundManager : MonoBehaviour, ISkillObserver
{
    public SoundConfigurationSO currentConfiguration;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    
    private Dictionary<string, SoundEffectSO> soundEffects = new Dictionary<string, SoundEffectSO>();
    private Dictionary<string, MusicTrackSO> musicTracks = new Dictionary<string, MusicTrackSO>();

    private void Awake()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        LoadSoundConfiguration();
    }

    public void LoadSoundConfiguration()
    {
        soundEffects.Clear();
        musicTracks.Clear();

        foreach (var effect in currentConfiguration.soundEffects)
        {
            soundEffects[effect.effectName] = effect;
        }

        foreach (var track in currentConfiguration.musicTracks)
        {
            musicTracks[track.trackName] = track;
        }
    }

    public void PlaySoundEffect(string effectName)
    {
        if (soundEffects.TryGetValue(effectName, out SoundEffectSO effect))
        {
            sfxSource.pitch = effect.pitch;
            sfxSource.PlayOneShot(effect.clip, effect.volume);
        }
        else
        {
            Debug.LogWarning($"Sound effect not found: {effectName}");
        }
    }

    // Other methods (PlayMusic, StopMusic, SetMusicVolume, SetSFXVolume) remain the same

    // Implementation of ISkillObserver
    public void OnSkillActivated(string skillName)
    {
        // Assuming skill names match sound effect names, or you have a mapping
        PlaySoundEffect(skillName);
    }
}

// EffectManager updated to use ObservableSkills
public class EffectManager : MonoBehaviour, ISkillObserver
{
    private Dictionary<string, SkillEffect> effectDatabase = new Dictionary<string, SkillEffect>();

    private void Awake()
    {
        LoadEffects();
    }

    private void Start()
    {
        var observableSkills = GetComponent<ObservableSkills>();
        if (observableSkills != null)
        {
            observableSkills.AddObserver(this);
        }
    }

    private void LoadEffects()
    {
        SkillEffect[] allEffects = Resources.LoadAll<SkillEffect>("SkillEffects");
        foreach (var effect in allEffects)
        {
            effectDatabase[effect.skillName] = effect;
        }
    }

    public void OnSkillActivated(string skillName)
    {
        PlayEffect(skillName);
    }

    private void PlayEffect(string skillName)
    {
        if (effectDatabase.TryGetValue(skillName, out SkillEffect effect))
        {
            // Play animation and particle effect
            if (!string.IsNullOrEmpty(effect.animationTrigger))
            {
                // Trigger animation
            }

            if (effect.particleEffect != null)
            {
                Instantiate(effect.particleEffect, transform.position, Quaternion.identity);
            }

            // Sound is now handled by SoundManager through the observer pattern
        }
    }
}

// Example of how to set up the system
public class GameSetup : MonoBehaviour
{
    public SoundConfigurationSO defaultSoundConfig;

    private void Start()
    {
        SetupSoundSystem();
        SetupSkillSystem();
    }

    private void SetupSoundSystem()
    {
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager == null)
        {
            soundManager = gameObject.AddComponent<SoundManager>();
        }
        soundManager.currentConfiguration = defaultSoundConfig;
        soundManager.LoadSoundConfiguration();
    }

    private void SetupSkillSystem()
    {
        var observableSkills = GetComponent<ObservableSkills>();
        if (observableSkills == null)
        {
            observableSkills = gameObject.AddComponent<ObservableSkills>();
        }

        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            observableSkills.AddObserver(soundManager);
        }

        var effectManager = GetComponent<EffectManager>();
        if (effectManager == null)
        {
            effectManager = gameObject.AddComponent<EffectManager>();
        }
        observableSkills.AddObserver(effectManager);
    }
}
