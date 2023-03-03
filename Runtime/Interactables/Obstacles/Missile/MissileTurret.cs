using UnityEngine;

namespace SpeedrunSim
{
    public class MissileTurret : MonoBehaviour
    {
        [SerializeField] Transform missileSpawnPoint;
        [SerializeField] PlayRandomClip shotSound;
        
        public void ShootMissile()
        {
            MissilePool.instance.ShootMissile(missileSpawnPoint);
            shotSound.Play();
        }
    }
}
