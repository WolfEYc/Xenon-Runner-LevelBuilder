using UnityEngine;
using UnityEngine.Pool;

namespace SpeedrunSim
{
    public class MissilePool : Singleton<MissilePool>
    {
        [SerializeField] Missile missilePrefab;
        [SerializeField] Timer explosionPrefab;
        
        ObjectPool<Missile> _missilePool;
        ObjectPool<Timer> _explosionPool;
        [SerializeField] Transform missleTransform;
        [SerializeField] Transform explosionTransform;

        protected override void Awake()
        {
            base.Awake();
            _missilePool = new ObjectPool<Missile>(
                CreateMissile,
                GetMissile,
                ReleaseMissile);
            _explosionPool = new ObjectPool<Timer>(
                CreateExplosion,
                GetExplosion,
                ReleaseExplosion);
        }

        Missile CreateMissile()
        {
            return Instantiate(missilePrefab, missleTransform);
        }

        Timer CreateExplosion()
        {
            Timer explosion = Instantiate(explosionPrefab, explosionTransform);
            
            explosion.Expired += () => _explosionPool.Release(explosion);
            var playRandomExplosion = explosion.GetComponent<PlayRandomClip>();
            var audioSource = explosion.GetComponent<AudioSource>();
            
            explosion.Started += () =>
            {
                playRandomExplosion.Play();
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            };
            
            
            playRandomExplosion.Play();
            
            return explosion;
        }

        void GetMissile(Missile missile)
        {
            missile.gameObject.SetActive(true);
        }

        void GetExplosion(Timer explosion)
        {
            explosion.gameObject.SetActive(true);
        }

        void ReleaseMissile(Missile missile)
        {
            missile.gameObject.SetActive(false);
        }

        void ReleaseExplosion(Timer explosion)
        {
            explosion.gameObject.SetActive(false);
        }

        public void ShootMissile(Transform origin)
        {
            _missilePool.Get().ShootMissile(origin);
        }

        public void Release(Missile missile)
        {
            _missilePool.Release(missile);
        }

        public void ExplodeAtPoint(Transform deathTransform, bool killedPlayer)
        {
            var explosion = _explosionPool.Get();
            explosion.transform.SetPositionAndRotation(deathTransform.position, deathTransform.rotation);

            if (killedPlayer)
            {
                explosion.GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Custom;
            }
        }
    }
}
