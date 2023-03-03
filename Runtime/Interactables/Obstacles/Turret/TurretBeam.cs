using UnityEngine;
using UnityEngine.VFX;

namespace SpeedrunSim
{
    [RequireComponent(typeof(LineRenderer))]
    public class TurretBeam : MonoBehaviour
    {
        LineRenderer _lr;
        public float windupProgress;

        Color _laserColor;
        
        [SerializeField] TurretVisible visiblity;
        [SerializeField] PlayRandomClip shotClip;
        [SerializeField] VisualEffect shotHitFX;
        
        Transform _transform;

        void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _transform = transform;
            _laserColor = Color.white;
        }
        

        void Update()
        {
            var pos = _transform.position;
            bool hitSomethin = PerformRaycast(out RaycastHit hit);

            _lr.SetPosition(1, new Vector3
            {
                z = hitSomethin && !hit.rigidbody.AmPlayer() ? Vector3.Distance(hit.point, pos) : visiblity.range
            });
            
            _laserColor.a = windupProgress;

            _lr.startColor = _laserColor;
            _lr.endColor = _laserColor;
        }

        bool PerformRaycast(out RaycastHit hit)
        {
            return Physics.Raycast(
                _transform.position,
                _transform.forward,
                out hit,
                visiblity.range,
                visiblity.shootable);
        }

        public void Shoot()
        {
            if(!PerformRaycast(out RaycastHit hit)) return;
            
            hit.collider.GetComponent<IDamageable>()?.Damage();

            if (!hit.collider.AmPlayer()) return;
            
            shotClip.Play();
            shotHitFX.transform.position = hit.point;
            shotHitFX.Play();
        }
    }
}
