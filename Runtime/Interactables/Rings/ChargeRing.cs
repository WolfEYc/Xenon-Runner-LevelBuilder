using System;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace SpeedrunSim
{
    public class ChargeRing : MonoBehaviour, ITriggerable, ICollisionListener
    {
        [SerializeField] VisualEffect vfx;
        [SerializeField] PlayRandomClip zapSound;
        [SerializeField] AudioSource staticSound;
        [SerializeField] float spawnRateDisingaged;
        [SerializeField] float spawnRateEngaged;
        [SerializeField] float spawnRateBeamsEngaged;
        [SerializeField] float audioLevelDisingaged;
        [SerializeField] GameObject btext;
        public float chargeTTL;
        
        bool _used;

        static readonly ExposedProperty SpawnRateStatic = "SpawnRateStatic";
        static readonly ExposedProperty SpawnRateBeams = "SpawnRateBeams";

        public static event Action<ChargeRing> Triggered;
        
        void OnTriggerEnter(Collider other)
        {
            if(_used) return;
            _used = true;
            
            zapSound.Play();
            staticSound.volume = 1f;

            if (btext != null)
            {
                btext.SetActive(true);
            }

            vfx.SetFloat(SpawnRateStatic, spawnRateEngaged);
            vfx.SetFloat(SpawnRateBeams, spawnRateBeamsEngaged);
            
            Triggered?.Invoke(this);
        }
        
        public void RespawnPointChanged()
        {
            staticSound.Stop();
            staticSound.volume = audioLevelDisingaged;
            vfx.SetFloat(SpawnRateStatic, spawnRateDisingaged);
            vfx.SetFloat(SpawnRateBeams, 0f);
            _used = false;
            
            if (btext != null)
            {
                btext.SetActive(false);
            }
        }

        public void Trigger(Collider other)
        {
            OnTriggerEnter(other);
        }

        public void OnTriggerEnterOccurred(Collider other)
        {
            OnTriggerEnter(other);
        }
    }
}
