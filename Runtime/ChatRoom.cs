#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    /// <summary>
    /// Clan Chat Room aka Group Chat Room
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is returned by the API.Clans.Client.JoinChatRoom and similar methods
    /// </para>
    /// <para>
    /// You should listen on the API.Clans.Client.EventChatMessageRecieved to recieve clan chat messages.
    /// That event will be raised for any clan you have joined the chat for API.Clans.Client.JoinChatRoom or by calling the Clan.JoinChat member on a clan returned from the API.Clans.CLient.GetClans()
    /// </para>
    /// </remarks>
    [Serializable]
    public struct ChatRoom : IEquatable<ChatRoom>
    {
        public Clan clan;
        public CSteamID id;
        public EChatRoomEnterResponse enterResponse;

        public UserData[] Members => API.Clans.Client.GetChatMembers(clan);
        /// <summary>
        /// Checks if the Steam Group chat room is open in the Steam UI.
        /// </summary>
        public bool IsOpenInSteam => API.Clans.Client.IsClanChatWindowOpenInSteam(id);

        /// <summary>
        /// Sends a message to a Steam group chat room.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(string message) => SteamFriends.SendClanChatMessage(id, message);
        /// <summary>
        /// Opens this chat in the Steam Overlay
        /// </summary>
        /// <returns></returns>
        public bool OpenChatWindowInSteam() => API.Clans.Client.OpenChatWindowInSteam(id);
        
        public void Leave() => API.Clans.Client.LeaveChatRoom(id);

    #region Boilerplate
        public bool Equals(ChatRoom other)
        {
            return clan == other.clan && id == other.id;
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ChatRoom))
            {
                var other = (ChatRoom)obj;
                return Equals(other);
            }
            else
                return base.Equals(obj);
        }
        public override int GetHashCode() => clan.GetHashCode() ^ id.GetHashCode();

        public static bool operator ==(ChatRoom l, ChatRoom r) => l.Equals(r);
        public static bool operator !=(ChatRoom l, ChatRoom r) => !l.Equals(r);
    #endregion
    }
#elif FACEPUNCH

    [System.Obsolete("You are useing Facepunch which does not support Clan chat, if you require this feature then remove Facepunch and install Steamworks.NET")]
    public struct ChatRoom
    {
    }
#endif
}
#endif