using UnityEngine;

namespace SpeedrunSim
{
    [RequireComponent(typeof(AudioSource))]
    public class VoiceLiner : Singleton<VoiceLiner>
    {
        AudioSource _audioSource;

        protected override void Awake()
        {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
        }


        public void PlayClip(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Stop();
            _audioSource.Play();
        }
    }
}
