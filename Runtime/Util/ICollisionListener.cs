using UnityEngine;

namespace SpeedrunSim
{
    public interface ICollisionListener
    {
        void OnTriggerEnterOccurred(Collider collider);
    }
}
