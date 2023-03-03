using System;
using UnityEngine;

namespace SpeedrunSim
{
    public class Timer : MonoBehaviour, IActionOnDestroyed
    {
        public event Action Expired;
        public event Action Started;

        public float endTime { get; private set; }
        public float requiredTime { get; private set; }
        public bool active { get; private set; }

        [SerializeField] float ttl;

        [SerializeField] bool startOnEnable;

        void OnEnable()
        {
            if(!startOnEnable) return;
            StartCountdown(ttl);
        }

        public void StartCountdown(float time)
        {
            requiredTime = time;
            endTime = Time.time + requiredTime;
            
            if (!active)
            {
                TimerMaster.instance.AddTimer(this);
                active = true;
            }
            
            Started?.Invoke();
        }

        public void StopCountdown()
        {
            if(!active) return;
            TimerMaster.instance.RemoveTimer(this);
            active = false;
        }

        public void Expire()
        {
            active = false;
            Expired?.Invoke();
        }

        public void OnDestroyed()
        {
            StopCountdown();
        }
    }
}
