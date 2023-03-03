using UnityEngine;

namespace SpeedrunSim
{
    public interface ITriggerable
    {
        void Trigger(Collider other);
    }
}
