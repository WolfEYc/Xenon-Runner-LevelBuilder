using System;
using UnityEngine;

namespace SpeedrunSim
{
    public class EndRing : MonoBehaviour, ICollisionListener, ITriggerable
    {
        public static event Action Completion;
        
        void OnTriggerEnter(Collider other)
        {
            //Can maybe do other stuff here perhaps
            
            Completion?.Invoke();
        }

        public void OnTriggerEnterOccurred(Collider other)
        {
            OnTriggerEnter(other);
        }

        public void Trigger(Collider other)
        {
            OnTriggerEnter(other);
        }
    }
}
