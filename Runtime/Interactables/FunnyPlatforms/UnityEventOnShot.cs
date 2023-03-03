using UnityEngine;
using UnityEngine.Events;

namespace SpeedrunSim
{
    
    public class UnityEventOnShot : MonoBehaviour, IDamageable
    {
        [SerializeField] UnityEvent onShot;

        public void Damage()
        {
            onShot.Invoke();
        }
    }
}
