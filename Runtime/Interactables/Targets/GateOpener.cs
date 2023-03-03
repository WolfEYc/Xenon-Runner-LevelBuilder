using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace SpeedrunSim
{
    public class GateOpener : MonoBehaviour, IDamageable, INudger
    {
        public List<IDamageable> Damageables;
        [SerializeField] Animator animator;
        [SerializeField] UnityEvent onDamaged;
        
        static readonly int Damaged = Animator.StringToHash("damaged");

        void Awake()
        {
            Damageables = new List<IDamageable>();
        }
        
        public void Damage()
        {
            foreach (IDamageable damageable in Damageables)
            {
                damageable.Damage();
            }

            animator.SetTrigger(Damaged);
            onDamaged.Invoke();
        }

        public Vector3 NudgeDir(Vector3 forward)
        {
            return forward;
        }
    }
}
