using System;
using UnityEngine;

namespace SpeedrunSim
{
    [RequireComponent(typeof(Animator))]
    public class TurretVisible : MonoBehaviour
    {
        Animator _animator;
        [SerializeField] Transform visibilityChecker;
        public float range;

        public LayerMask shootable;

        public Collider PlayerCollider { get; private set; }
        static readonly int PlayerVisible = Animator.StringToHash("PlayerVisible");
        Vector3 _origin;

        public static event Action<TurretVisible> VisibilityDi;
        public event Action ColliderAssigned;

        public void SetCollider(Collider col)
        {
            PlayerCollider = col;
            ColliderAssigned?.Invoke();
        }
        
        void Awake()
        {
            _animator = GetComponent<Animator>();
            _origin = visibilityChecker.position;
        }

        void Start()
        {
            VisibilityDi?.Invoke(this);
        }
        
        void FixedUpdate()
        {
            bool hit = Physics.Raycast(
                _origin,
                PlayerCollider.ClosestPoint(_origin) - _origin,
                out RaycastHit rayhit,
                range,
                shootable
            );

            _animator.SetBool(PlayerVisible, hit && rayhit.collider.AmPlayer());
        }
    }
    
    
}
