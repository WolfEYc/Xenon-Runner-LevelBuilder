#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct LobbyMember
    {
        public Lobby lobby;
        public UserData user;

        public string this[string metadataKey]
        {
            get
            {
                return API.Matchmaking.Client.GetLobbyMemberData(lobby, user, metadataKey);
            }
            set
            {
                if (user == API.User.Client.Id)
                    API.Matchmaking.Client.SetLobbyMemberData(lobby, metadataKey, value);
            }
        }

        public bool IsReady
        {
            get => this[Lobby.DataReady] == "true";
            set => this[Lobby.DataReady] = value.ToString().ToLower();
        }

        public string GameVersion
        {
            get => this[Lobby.DataVersion];
            set => this[Lobby.DataVersion] = value;
        }

        public void Kick() => lobby.KickMember(user);
    }
}
#endif