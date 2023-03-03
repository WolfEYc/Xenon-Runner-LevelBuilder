using UnityEngine;

namespace SpeedrunSim
{
    public class MissileMovement : MonoBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] float accel, maxVel;
        
        Rigidbody _playerRb;
        Transform _transform;
        Vector3 Dir2Player => _playerRb.position - rb.position;

        void Start()
        {
            _transform = transform;
            _playerRb = ExtensionMethods.PlayerRb;
        }

        void FixedUpdate()
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity + Dir2Player * (accel * Time.fixedDeltaTime), maxVel);
            _transform.up = rb.velocity;
        }

        public void ShootMissile(Transform origin)
        {
            rb.position = origin.position;
            rb.rotation = origin.rotation;
            rb.velocity = maxVel * origin.up;
        }
    }
}
