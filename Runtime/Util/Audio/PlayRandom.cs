using UnityEngine;

namespace SpeedrunSim
{
    public class PlayRandom : MonoBehaviour
    {
        AudioSource[] _playSources;

        void Awake()
        {
            _playSources = GetComponentsInChildren<AudioSource>();
        }

        public void PlayRandomSound()
        {
            _playSources.RandomElement().Play();
        }
    }
}
