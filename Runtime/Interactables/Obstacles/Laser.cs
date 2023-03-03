using UnityEngine;

namespace SpeedrunSim
{
    public class Laser : MonoBehaviour, ITriggerable
    {
        [SerializeField] FXResponder fxResponder;
        
        
        void OnTriggerEnter(Collider other)
        {
            Trigger(other);
        }

        public void Trigger(Collider other)
        {
            fxResponder.Damage();
            other.GetComponent<IDamageable>()?.Damage();
        }
    }
}
