#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [Serializable]
    public struct LobbyChatMsg
    {
        public Lobby lobby;
        public EChatEntryType type;
        public UserData sender;
        public byte[] data;
        public DateTime recievedTime;
        public string Message => ToString();
        public override string ToString()
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }

        public T FromJson<T>() => UnityEngine.JsonUtility.FromJson<T>(ToString());
    }
#elif FACEPUNCH
    [Serializable]
    public struct LobbyChatMsg
    {
        public Lobby lobby;
        public UserData sender;
        public DateTime recievedTime;
        public string message;
    }
#endif
}
#endif