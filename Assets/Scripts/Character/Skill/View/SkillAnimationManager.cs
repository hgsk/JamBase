using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class SkillEvent : UnityEvent<string> { }

namespace Skill.View
{

    [CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill")]
    public class Skill : ScriptableObject
    {
        public string skillName;
        public float cooldownDuration;
        public AnimationClip skillAnimation;
        //public TimelineAsset skillTimeline;
        public AudioClip skillSound;
        internal object skillTimeline;
    }

    [RequireComponent(typeof(Animator), typeof(PlayableDirector))]
    public class CharacterAnimationController : MonoBehaviour
    {
        private Animator animator;
        private PlayableDirector director;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            director = GetComponent<PlayableDirector>();
        }

        public void PlaySkillAnimation(AnimationClip clip)
        {
            animator.Play(clip.name);
        }

        public void PlaySkillTimeline(TimelineAsset timeline)
        {
            director.playableAsset = timeline;
            director.Play();
        }

        internal void PlaySkillTimeline(object skillTimeline)
        {
        }
    }


    public class SkillViewController : MonoBehaviour
    {
        public SkillManager skillManager;
        public CharacterAnimationController animationController;
        public SoundManager soundManager;

        private void Start()
        {
            if (skillManager == null) skillManager = GetComponent<SkillManager>();
            if (animationController == null) animationController = GetComponent<CharacterAnimationController>();
            if (soundManager == null) soundManager = GetComponent<SoundManager>();

            SetupSkillSystem();
        }

        private void SetupSkillSystem()
        {
            Skill[] skillsArray = Resources.LoadAll<Skill>("Skills");
            foreach (var skill in skillsArray)
            {
                //skillManager.AddSkill(skill);
            }

            skillManager.OnSkillUsed.AddListener(HandleSkillUsed);
        }

        private void HandleSkillUsed()
        {
        }

        private void HandleSkillUsed(string skillName)
        {
            Skill skill = (Skill)skillManager.skills.First(s => s.Name == skillName);
            if (skill != null)
            {
                if (skill.skillAnimation != null)
                {
                    animationController.PlaySkillAnimation(skill.skillAnimation);
                }

                if (skill.skillTimeline != null)
                {
                    animationController.PlaySkillTimeline(skill.skillTimeline);
                }

                if (skill.skillSound != null)
                {
                    soundManager.PlaySound(skill.skillSound);
                }

                Debug.Log($"Skill used: {skillName}");
            }
        }

        // This method could be called by UI buttons or input system
        public void UseSkill(string skillName)
        {
            skillManager.UseSkill(skillName);
        }
    }

    // Example of a custom PlayableBehaviour for Timeline
    public class SkillEffectPlayableBehaviour : PlayableBehaviour
    {
        public GameObject effectPrefab;
        private GameObject spawnedEffect;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (effectPrefab != null)
            {
                spawnedEffect = GameObject.Instantiate(effectPrefab);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (spawnedEffect != null)
            {
                GameObject.Destroy(spawnedEffect);
            }
        }
    }

    // Custom TrackAsset for Timeline
    //[TrackClipType(typeof(SkillEffectPlayableAsset))]
    public class SkillEffectTrack { }// TrackAsset { }

    // Custom PlayableAsset for Timeline
    public class SkillEffectPlayableAsset : PlayableAsset
    {
        public GameObject effectPrefab;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SkillEffectPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.effectPrefab = effectPrefab;
            return playable;
        }
    }

}