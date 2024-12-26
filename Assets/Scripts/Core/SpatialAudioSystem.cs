using UnityEngine;
namespace Core.SpatialAudio
{

    public interface ISpatialAudioCalculator

    {

        (float leftVolume, float rightVolume) CalculateSpatialVolume(float distance, float angle, SpatialAudioSource.AudioProperties audioProperties);

    }



    public class SpatialAudioCalculator : ISpatialAudioCalculator

    {

        public (float leftVolume, float rightVolume) CalculateSpatialVolume(float distance, float angle, SpatialAudioSource.AudioProperties audioProperties)

        {

            // Implement your spatial audio calculation logic here

            float leftVolume = audioProperties.volume * (1 - angle / 180f);

            float rightVolume = audioProperties.volume * (1 + angle / 180f);

            return (leftVolume, rightVolume);

        }

    }

    [RequireComponent(typeof(AudioSource))]
    public class SpatialAudioSource : MonoBehaviour
    {
        [SerializeField]
        [System.Serializable]
        public class AudioProperties
        {
            public float volume;
            public float pitch;
        }

        [SerializeField]
        private AudioProperties audioProperties;

        [SerializeField]
        private bool debugMode;

        private AudioSource audioSource;
        private ISpatialAudioCalculator audioCalculator;
        private Transform listenerTransform;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioCalculator = new SpatialAudioCalculator();
            listenerTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (!audioSource.isPlaying) return;

            Vector3 toSource = transform.position - listenerTransform.position;
            float distance = toSource.magnitude;
            float angle = Vector3.SignedAngle(
                listenerTransform.forward,
                toSource,
                Vector3.up);

            (float leftVolume, float rightVolume) = audioCalculator.CalculateSpatialVolume(
                distance,
                angle,
                audioProperties);

            // オーディオソースのステレオパンとボリュームを更新
            audioSource.panStereo = (rightVolume - leftVolume) / 2f;
            audioSource.volume = (leftVolume + rightVolume) / 2f;

            if (debugMode)
            {
                Debug.LogFormat(
                    "Distance: {0:F2}, Angle: {1:F2}, Left: {2:F2}, Right: {3:F2}",
                    distance, angle, leftVolume, rightVolume);
            }
        }
    }
}