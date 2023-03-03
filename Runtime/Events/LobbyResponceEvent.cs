#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public class LobbyResponceEvent : UnityEvent<Steamworks.EChatRoomEnterResponse> { }

    [System.Serializable]
    public class EResultEvent : UnityEvent<Steamworks.EResult> { }
}
#endif