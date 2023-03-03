using UnityEngine;

namespace SpeedrunSim
{
    public class CollisionReporter : MonoBehaviour, ITriggerable
    {
        ICollisionListener _collisionListener;

        void Awake()
        {
            _collisionListener = GetComponentInParent<ICollisionListener>();
        }

        void OnTriggerEnter(Collider other)
        {
            _collisionListener?.OnTriggerEnterOccurred(other);
        }

        public void Trigger(Collider other)
        {
            OnTriggerEnter(other);
        }
    }
}
