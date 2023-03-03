using UnityEngine;

namespace SpeedrunSim
{
    public class MapsDisplay : MonoBehaviour
    {
        public CommunityMapDisplay[] MapDisplays { get; private set; }

        void Awake()
        {
            MapDisplays = GetComponentsInChildren<CommunityMapDisplay>();
        }
    }
}
