using UnityEngine;

namespace SpeedrunSim
{
    public class ExplosionForce : MonoBehaviour
    {
        [SerializeField] float explosionRange;
        [SerializeField] float explosionForce;
        public bool explodeOnEnable;

        float _sqrRange;

        void Awake()
        { 
            _sqrRange = explosionRange * explosionRange;
        }

        void OnEnable()
        {
            if(!explodeOnEnable) return;
            Explode();
        }

        public void Explode()
        {
            var playerRb = ExtensionMethods.PlayerRb;

            var offset = playerRb.position - transform.position;
            
            float sqrDist = offset.sqrMagnitude;
            
            if(sqrDist > _sqrRange) return;
            
            playerRb.AddForce(Mathf.Clamp01(1f / sqrDist) * explosionForce * offset.normalized, ForceMode.VelocityChange);
        }
    }
}
