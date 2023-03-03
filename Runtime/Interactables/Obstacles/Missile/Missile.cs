using System;
using UnityEngine;

namespace SpeedrunSim
{
    public class Missile : MonoBehaviour, IDamageable, INudger
    {
        [SerializeField] Timer timer;
        [SerializeField] Timer launchTimer;
        [SerializeField] MissileMovement movement;
        [SerializeField] TrailRenderer tr;

        bool _killedPlayer;

        public static Action KillAllMissiles;

        void Awake()
        {
            timer.Expired += Damage;
            launchTimer.Expired += EnableBoom;
            KillAllMissiles += Damage;
        }

        void EnableBoom()
        {
            movement.enabled = true;
        }

        public void ShootMissile(Transform origin)
        {
            transform.SetPositionAndRotation(origin.position, origin.rotation);
            
            movement.ShootMissile(origin);
        }
        
        public void Damage()
        {
            if(!movement.enabled) return;
            
            timer.StopCountdown();
            launchTimer.StopCountdown();
            
            movement.enabled = false;

            MissilePool.instance.ExplodeAtPoint(transform, _killedPlayer);
            _killedPlayer = false;

            tr.Clear();
            
            MissilePool.instance.Release(this);
        }

        void OnTriggerEnter(Collider other)
        {
            if(!movement.enabled) return;

            _killedPlayer = other.AmPlayer();
            
            Damage();

            other.GetComponent<IDamageable>()?.Damage();
        }

        public Vector3 NudgeDir(Vector3 forward)
        {
            return forward;
        }

        void OnDestroy()
        {
            KillAllMissiles -= Damage;
        }
    }
}
