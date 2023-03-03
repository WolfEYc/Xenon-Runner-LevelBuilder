using UnityEngine;

namespace SpeedrunSim
{
    public class TeleportTarget : MonoBehaviour, IDamageable, INudger
    {
        [SerializeField] float pushForce;
        
        public void Damage()
        {
            var rb = ExtensionMethods.PlayerRb;
            var self = transform;
            rb.position = self.position;
            rb.velocity = self.forward * (rb.velocity.magnitude + pushForce);
        }

        public Vector3 NudgeDir(Vector3 forward)
        {
            return transform.forward;
        }
    }
}
