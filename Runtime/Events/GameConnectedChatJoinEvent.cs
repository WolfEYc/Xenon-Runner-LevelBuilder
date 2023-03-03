#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class GameConnectedChatJoinEvent : UnityEvent<ChatRoom, CSteamID> { }
#elif FACEPUNCH
    [System.Serializable]
    [System.Obsolete("You are useing Facepunch which does not support Clan chat, if you require this feature then remove Facepunch and install Steamworks.NET")]
    public class GameConnectedChatJoinEvent : UnityEvent<ChatRoom, SteamId> { }
#endif
}
#endif