using UnityEngine;

namespace SpeedrunSim
{
    public class DashTarget : MonoBehaviour, IDamageable, INudger
    {
        [SerializeField] float boostForce;

        public void Damage()
        {
            var rb = ExtensionMethods.PlayerRb;
            rb.velocity = (boostForce + rb.velocity.magnitude) * Vector3.Normalize(transform.position - rb.position);
        }

        public Vector3 NudgeDir(Vector3 forward)
        {
            return forward;
        }
    }
}
