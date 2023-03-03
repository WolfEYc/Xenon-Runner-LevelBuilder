using UnityEngine;

namespace SpeedrunSim
{
    public class TurretLookAt : MonoBehaviour
    {
        [SerializeField] float rotSpeed;
        [SerializeField] Transform yRotator;
        [SerializeField] Transform xRotator;
        [SerializeField] TurretVisible visibility;
        
        Vector3 _origin;
        Transform _playerTransform;
        

        Vector2 _currentRot;

        void Awake()
        {
            visibility.ColliderAssigned += VisibilityOnColliderAssigned;
        }

        void VisibilityOnColliderAssigned()
        {
            _playerTransform = visibility.PlayerCollider.transform;
        }

        void Start()
        {
            _origin = transform.position;
        }

        void OnEnable()
        {
            _currentRot = new Vector2(xRotator.eulerAngles.x, yRotator.eulerAngles.y);
        }

        void Update()
        {
            var lookRot = Quaternion.LookRotation(_playerTransform.position - _origin).eulerAngles;

            float dTime = Time.deltaTime;
            _currentRot.x = Mathf.MoveTowardsAngle(_currentRot.x, lookRot.x, rotSpeed * dTime);
            _currentRot.y = Mathf.MoveTowardsAngle(_currentRot.y, lookRot.y, rotSpeed * dTime);

            xRotator.localRotation = Quaternion.AngleAxis(_currentRot.x, Vector3.right);
            yRotator.localRotation = Quaternion.AngleAxis(_currentRot.y, Vector3.up);
        }
    }
}
