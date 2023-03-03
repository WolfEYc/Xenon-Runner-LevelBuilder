using UnityEngine;

namespace SpeedrunSim
{
    public class TimeModify : MonoBehaviour
    {
        [SerializeField] PlayRandomClip timeSpeedSFX;
        [SerializeField] PlayRandomClip timeSlowSFX;    
        
        public float timescale;
        
        void Update()
        {
            Time.timeScale = timescale;
        }

        public void PlaySpeedUpSFX()
        {
            timeSpeedSFX.Play();
        }

        public void PlaySlowDownSFX()
        {
            timeSlowSFX.Play();
        }
    }
}
