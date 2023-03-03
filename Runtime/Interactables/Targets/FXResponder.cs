using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace SpeedrunSim
{
    [RequireComponent(typeof(VisualEffect), typeof(PlayRandomClip))]
    public class FXResponder : MonoBehaviour, IDamageable
    {
        public VisualEffect VFX { get; private set; }
        PlayRandomClip _playRandomClip;
        static readonly ExposedProperty ShotEvent = "Shot";

        [SerializeField] UnityEvent onShot;
        
        void Awake()
        {
            VFX = GetComponent<VisualEffect>();
            _playRandomClip = GetComponent<PlayRandomClip>();
        }

        public void Damage()
        {
            onShot.Invoke();
            VFX.SendEvent(ShotEvent);
            _playRandomClip.Play();
        }
    }
}
