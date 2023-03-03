#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public struct UserLobbyLeaveData
    {
        public UserData user;
        public Steamworks.EChatMemberStateChange state;
    }
}
#endif