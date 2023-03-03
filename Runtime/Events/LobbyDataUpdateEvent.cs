#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class LobbyDataUpdateEvent : UnityEvent<LobbyDataUpdate_t> { }
#elif FACEPUNCH
    [System.Serializable]
    public class LobbyDataUpdateEvent : UnityEvent<LobbyDataUpdate> { }

    [System.Serializable]
    public struct LobbyDataUpdate
    {
        public Lobby lobby;
        public LobbyMember? member;
    }
#endif
}
#endif