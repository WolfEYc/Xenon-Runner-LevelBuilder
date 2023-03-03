using System;
using UnityEngine;

namespace SpeedrunSim
{
    public class SpawnRing : MonoBehaviour
    {
        public static event Action SpawnReset;
        public static Vector3 SpawnPos;

        void Awake()
        {
            SpawnPos = transform.position;
        }

        void Start()
        {
            SpawnReset?.Invoke();
        }
    }
}