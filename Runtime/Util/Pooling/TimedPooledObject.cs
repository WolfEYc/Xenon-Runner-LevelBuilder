using UnityEngine;

namespace SpeedrunSim
{
    [RequireComponent(typeof(Timer))]
    public abstract class TimedPooledObject<T> : PooledObject<T> where T : TimedPooledObject<T>
    {
        protected Timer Timer;
        
        protected override void Awake()
        {
            base.Awake();
            Timer = GetComponent<Timer>();
            Timer.Expired += Release;
        }
    }
}
