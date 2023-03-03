using UnityEngine;

namespace SpeedrunSim
{
    public class GunTurret : MonoBehaviour
    {
        [SerializeField] TurretBeam beam;


        public void Shoot()
        {
            beam.Shoot();
        }
    }
}
