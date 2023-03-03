using UnityEngine;

namespace SpeedrunSim
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayRandomClip : MonoBehaviour
    {
        AudioSource _audioSource;

        [SerializeField] AudioClip[] clips;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Play()
        {
            _audioSource.clip = clips.RandomElement();
            _audioSource.Play();
        }
    }
}
