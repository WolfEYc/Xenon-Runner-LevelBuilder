#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [Serializable]
    public struct Lobby : IEquatable<CSteamID>, IEquatable<ulong>
    {
        public CSteamID id;
        public ulong SteamId
        {
            get => id.m_SteamID;
            set => id = new CSteamID(value);
        }
        public AccountID_t AccountId
        {
            get => id.GetAccountID();
            set
            {
                id = new CSteamID(value, 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
            }
        }
        public uint FriendId
        {
            get => AccountId.m_AccountID;
            set
            {
                id = new CSteamID(new AccountID_t(value), EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
            }
        }
        /// <summary>
        /// Is this Lobby value a valid value.
        /// This does not indicate it is a lobby simply that structurally the data is possibly a lobby
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (id == CSteamID.Nil
                    || id.GetEAccountType() != EAccountType.k_EAccountTypeChat
                    || id.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// Get or set the lobby name
        /// </summary>
        /// <remarks>
        /// <para>
        /// The lobby name is a metadata field whoes key is "name". Setting this field will update the lobby metadata accordinly and this update will be reflected to all members.
        /// Only the owner of the lobby can set this value.
        /// </para>
        /// </remarks>
        public string Name
        {
            get => this[DataName];
            set => this[DataName] = value;
        }
        /// <summary>
        /// The current owner of the lobby.
        /// </summary>
        public LobbyMember Owner
        {
            get => new LobbyMember { lobby = this, user = API.Matchmaking.Client.GetLobbyOwner(id) };
            set => API.Matchmaking.Client.SetLobbyOwner(id, value.user);
        }
        /// <summary>
        /// The member data for this user
        /// </summary>
        public LobbyMember Me => new LobbyMember { lobby = this, user = API.User.Client.Id };
        [Obsolete("Please use Me instead.")]
        public LobbyMember User => Me;
        /// <summary>
        /// The collection of all members of this lobby including the owner of the lobby.
        /// </summary>
        public LobbyMember[] Members => API.Matchmaking.Client.GetLobbyMembers(id);
        /// <summary>
        /// True if the data type metadata is set
        /// </summary>
        public bool IsTypeSet => !string.IsNullOrEmpty(API.Matchmaking.Client.GetLobbyData(id, DataType));
        /// <summary>
        /// Returns the type of the lobby if set, if not set this will default to Private, you can check if the type is set with <see cref="IsTypeSet"/>
        /// </summary>
        public ELobbyType Type
        {
            get 
            {
                if (int.TryParse(API.Matchmaking.Client.GetLobbyData(id, DataType), out int enumVal))
                {
                    return (ELobbyType)enumVal;
                }
                else
                    return ELobbyType.k_ELobbyTypePrivate;
            }
            set => API.Matchmaking.Client.SetLobbyType(id, value);
        }
        /// <summary>
        /// Gets or sets the version of the game the lobby is configured for ... this should match the owners version
        /// </summary>
        public string GameVersion
        {
            get => this[DataVersion];
            set => this[DataVersion] = value;
        }
        /// <summary>
        /// Is the user the host of this lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyOwner(CSteamID)"/> and compares the results to <see cref="SteamUser.GetSteamID()"/>.
        /// This returns true if the provided lobby ID is a legitimate ID and if Valve indicates that the lobby has members and if the owner of the lobby is the current player.
        /// </para>
        /// </remarks>
        public bool IsOwner
        {
            get
            {
                return SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(id);
            }
        }
        /// <summary>
        /// Indicates rather or not this lobby is a party lobby
        /// </summary>
        public bool IsGroup
        {
            get
            {
                return this[DataMode] == "Group";
            }
            set
            {
                if (IsOwner)
                {
                    if (value)
                    {
                        SetType(ELobbyType.k_ELobbyTypeInvisible);
                        this[DataMode] = "Group";
                    }
                    else
                    {
                        this[DataMode] = "General";
                    }
                }
            }
        }
        /// <summary>
        /// Does this lobby have a game server registered to it
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyGameServer(CSteamID, out uint, out ushort, out CSteamID)"/> and cashes the data to <see cref="GameServer"/>.
        /// It is not usually nessisary to check this value since the set game server callback from Steamworks will automatically update these values if the user was connected to the lobby when the set game server data was called.
        /// </para>
        /// </remarks>
        public bool HasServer => SteamMatchmaking.GetLobbyGameServer(id, out _, out _, out _);
        public LobbyGameServer GameServer => API.Matchmaking.Client.GetLobbyGameServer(id);
        /// <summary>
        /// Returns true if all of the players 'IsReady' is true
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determin if the players are ready to play the game.
        /// </para>
        /// </remarks>
        public bool AllPlayersReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return Members.Any(p => !p.IsReady) ? false : true;
            }
        }
        /// <summary>
        /// Returns true if all of the players 'IsReady' is false
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determin if all players have reset the ready flag such as when some change is made after a previous ready check had already passed.
        /// </para>
        /// </remarks>
        public bool AllPlayersNotReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return Members.Any(p => p.IsReady) ? false : true;
            }
        }
        public bool IsReady
        {
            get => API.Matchmaking.Client.GetLobbyMemberData(id, API.User.Client.Id, DataReady) == "true";
            set => API.Matchmaking.Client.SetLobbyMemberData(id, DataReady, value.ToString().ToLower());
        }
        public bool Full => API.Matchmaking.Client.GetLobbyMemberLimit(id) <= SteamMatchmaking.GetNumLobbyMembers(id);
        public int MaxMembers
        {
            get => API.Matchmaking.Client.GetLobbyMemberLimit(id);
            set => API.Matchmaking.Client.SetLobbyMemberLimit(id, value);
        }
        /// <summary>
        /// Read and write metadata values to the lobby
        /// </summary>
        /// <param name="metadataKey">The key of the value to be read or writen</param>
        /// <returns>The value of the key if any otherwise returns and empty string.</returns>
        public string this[string metadataKey]
        {
            get
            {
                return API.Matchmaking.Client.GetLobbyData(id, metadataKey);
            }
            set
            {
                API.Matchmaking.Client.SetLobbyData(id, metadataKey, value);
            }
        }
        /// <summary>
        /// Get the LobbyMember object for a given user
        /// </summary>
        /// <param name="id">The ID of the member to fetch</param>
        /// <param name="member">The member found</param>
        /// <returns>True if the user is a member of the lobby, false if they are not</returns>
        public bool GetMember(CSteamID id, out LobbyMember member) => API.Matchmaking.Client.GetMember(this, id, out member);
        /// <summary>
        /// Checks if a user is a member of this lobby
        /// </summary>
        /// <param name="id">The user to check for</param>
        /// <returns>True if they are, false if not</returns>
        public bool IsAMember(CSteamID id) => API.Matchmaking.Client.IsAMember(this, id);
        /// <summary>
        /// Updates the lobby type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool SetType(ELobbyType type) => API.Matchmaking.Client.SetLobbyType(id, type);
        public bool SetJoinable(bool makeJoinable) => API.Matchmaking.Client.SetLobbyJoinable(id, makeJoinable);
        /// <summary>
        /// Gets the dictionary of metadata values assigned to this lobby.
        /// </summary>
        public Dictionary<string, string> GetMetadata()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            var count = SteamMatchmaking.GetLobbyDataCount(id);

            for (int i = 0; i < count; i++)
            {
                SteamMatchmaking.GetLobbyDataByIndex(id, i, out string key, Constants.k_nMaxLobbyKeyLength, out string value, Constants.k_cubChatMetadataMax);
                result.Add(key, value);
            }

            return result;
        }
        /// <summary>
        /// Join this lobby
        /// </summary>
        /// <param name="callback">Handler(LobbyEnter_t result, bool IOError)</param>
        public void Join(Action<LobbyEnter_t, bool> callback)
        {
            API.Matchmaking.Client.JoinLobby(this, callback);
        }
        /// <summary>
        /// Leaves the current lobby if any
        /// </summary>
        public void Leave()
        {
            if (id == CSteamID.Nil)
                return;

            API.Matchmaking.Client.LeaveLobby(id);

            id = CSteamID.Nil;
        }
        public bool DeleteLobbyData(string dataKey) => API.Matchmaking.Client.DeleteLobbyData(id, dataKey);

        public bool InviteUserToLobby(UserData targetUser) => API.Matchmaking.Client.InviteUserToLobby(id, targetUser);

        public bool SendChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            byte[] MsgBody = System.Text.Encoding.UTF8.GetBytes(message);
            return SteamMatchmaking.SendLobbyChatMsg(id, MsgBody, MsgBody.Length);
        }
        public bool SendChatMessage(byte[] data)
        {
            if (data == null || data.Length < 1)
                return false;

            return SteamMatchmaking.SendLobbyChatMsg(id, data, data.Length);
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer(string address, ushort port, CSteamID gameServerId)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, GameServer.ipAddress, port, gameServerId);
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer(string address, ushort port)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, GameServer.ipAddress, port, CSteamID.Nil);
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer(CSteamID gameServerId)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, 0, 0, gameServerId);
        }
        /// <summary>
        /// <para>
        /// This overload uses the lobby owner's CSteamID as the server ID which is typical of P2P session.
        /// </para>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer()
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, 0, 0, API.User.Client.Id);
        }
        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberId"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public bool KickMember(CSteamID memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);

            if (kickList == null)
                kickList = string.Empty;

            if (!kickList.Contains("[" + memberId.ToString() + "]"))
                kickList += "[" + memberId.ToString() + "]";

            return API.Matchmaking.Client.SetLobbyData(id, DataKick, kickList);
        }
        public bool KickListContains(CSteamID memberId)
        {
            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);
            return kickList.Contains("[" + memberId.ToString() + "]");
        }
        public bool RemoveFromKickList(CSteamID memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);

            kickList = kickList.Replace("[" + memberId.ToString() + "]", string.Empty);

            return API.Matchmaking.Client.SetLobbyData(id, DataKick, kickList);
        }
        public bool ClearKickList()
        {
            if (!IsOwner)
                return false;

            return API.Matchmaking.Client.DeleteLobbyData(id, DataKick);
        }
        /// <summary>
        /// Use this sparingly it requires string parcing and is not performant
        /// </summary>
        /// <returns></returns>
        public CSteamID[] GetKickList()
        {
            var list = API.Matchmaking.Client.GetLobbyData(id, DataKick);
            if (!string.IsNullOrEmpty(list))
            {
                var sArray = list.Split(new string[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                var resultList = new List<CSteamID>();
                for (int i = 0; i < sArray.Length; i++)
                {
                    if (ulong.TryParse(sArray[i].Replace("[", string.Empty).Replace("]", string.Empty), out ulong id))
                        resultList.Add(new CSteamID(id));
                }

                return resultList.ToArray();
            }
            else
                return new CSteamID[0];
        }
        /// <summary>
        /// Sets metadata for the player on the first lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetMemberMetadata(string key, string value)
        {
            API.Matchmaking.Client.SetLobbyMemberData(id, key, value);
        }
        /// <summary>
        /// Returns the metadata field of the local user
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMemberMetadata(string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, API.User.Client.Id, key);
        }
        /// <summary>
        /// Returns the metadata field of the user indicated by <paramref name="memberId"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMemberMetadata(CSteamID memberId, string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, memberId, key);
        }
        /// <summary>
        /// Returns the metadata field of the user indicated by <paramref name="member"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMemberMetadata(LobbyMember member, string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, member.user, key);
        }
        public static Lobby Get(string accountId)
        {
            if (uint.TryParse(accountId, out uint result))
                return Get(result);
            else
                return CSteamID.Nil;
        }
        /// <summary>
        /// Get the lobby represented by this account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static Lobby Get(uint accountId) => new CSteamID(new AccountID_t(accountId), 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
        /// <summary>
        /// Get the lobby represented by this account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static Lobby Get(AccountID_t accountId) => new CSteamID(accountId, 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
        /// <summary>
        /// Get the lobby represented by this CSteamID value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Lobby Get(ulong id) => new Lobby { id = new CSteamID(id) };
        /// <summary>
        /// Get the lobby represented by this CSteamID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Lobby Get(CSteamID id) => new Lobby { id = id };
        /// <summary>
        /// Join the lobby represented by this account Id
        /// </summary>
        /// <param name="accountId">Must be a valid uint as a string</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(string accountId, Action<LobbyEnter_t, bool> callback) => API.Matchmaking.Client.JoinLobby(Get(accountId), callback);
        /// <summary>
        /// Join the lobby
        /// </summary>
        /// <param name="lobby">The lobby to join</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(Lobby lobby, Action<LobbyEnter_t, bool> callback) => API.Matchmaking.Client.JoinLobby(lobby, callback);
        /// <summary>
        /// Join the lobby represented by this account Id
        /// </summary>
        /// <param name="accountId">Must be a valid uint as a string</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(AccountID_t accountId, Action<LobbyEnter_t, bool> callback) => API.Matchmaking.Client.JoinLobby(Get(accountId), callback);

    #region Constants
        /// <summary>
        /// Standard metadata field representing the name of the lobby.
        /// This field is typically only used in lobby metadata
        /// </summary>
        public const string DataName = "name";
        /// <summary>
        /// Heathen standard metadata field representing the version of the game.
        /// This field is commonly used in lobby and member metadata
        /// </summary>
        public const string DataVersion = "z_heathenGameVersion";
        /// <summary>
        /// Heathen standard metadata field indicating that the user is ready to play.
        /// This field is commonly only used on member metadata
        /// </summary>
        public const string DataReady = "z_heathenReady";
        /// <summary>
        /// Heathen standard metadata field indicating that these users should leave the lobby.
        /// This is a string containing each CSteamID of members that should not join this lobby and if present should leave it.
        /// Data in this list is in the form of [ + CSteamID + ] e.g. [123456789][987654321] would indicate 2 users that should leave
        /// This field is commonly only used on lobby metadata
        /// </summary>
        public const string DataKick = "z_heathenKick";
        /// <summary>
        /// Heathen standard metadata field indicating the mode of the lobby e.g. group or general
        /// If this is blank its assumed to be general
        /// </summary>
        public const string DataMode = "z_heathenMode";
        /// <summary>
        /// Heathen standard metadata field indicating the type of lobby e.g. private, friend, public or invisible
        /// </summary>
        public const string DataType = "z_heathenType";
    #endregion

    #region Boilerplate
        public int CompareTo(CSteamID other)
        {
            return id.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return id.m_SteamID.CompareTo(other);
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public bool Equals(ulong other)
        {
            return id.m_SteamID.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.m_SteamID.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(Lobby l, Lobby r) => l.id == r.id;
        public static bool operator ==(CSteamID l, Lobby r) => l == r.id;
        public static bool operator ==(Lobby l, CSteamID r) => l.id == r;
        public static bool operator ==(Lobby l, ulong r) => l.id.m_SteamID == r;
        public static bool operator ==(ulong l, Lobby r) => l == r.id.m_SteamID;
        public static bool operator !=(Lobby l, Lobby r) => l.id != r.id;
        public static bool operator !=(CSteamID l, Lobby r) => l != r.id;
        public static bool operator !=(Lobby l, CSteamID r) => l.id != r;
        public static bool operator !=(Lobby l, ulong r) => l.id.m_SteamID != r;
        public static bool operator !=(ulong l, Lobby r) => l != r.id.m_SteamID;

        public static implicit operator CSteamID(Lobby c) => c.id;
        public static implicit operator Lobby(CSteamID id) => new Lobby { id = id };
        public static implicit operator ulong(Lobby id) => id.id.m_SteamID;
        public static implicit operator Lobby(ulong id) => new Lobby { id = new CSteamID(id) };

    #endregion
    }
#elif FACEPUNCH
    [Serializable]
    public struct Lobby : IEquatable<SteamId>, IEquatable<ulong>
    {
        public SteamId id;
        public ulong SteamId
        {
            get => id.Value;
            set => id = new SteamId { Value = value };
        }
        public uint AccountId
        {
            get => id.AccountId;
            set
            {
                var sId = new SteamId { Value = value };
                sId = ExtendedSteamId.SetAccountInstance(sId, 393216);
                sId = ExtendedSteamId.SetAccountID(sId, value);
                sId = ExtendedSteamId.SetEAccountType(sId, EAccountType.Chat);
                id = sId;
            }
        }
        public uint FriendId
        {
            get => id.AccountId;
            set
            {
                var sId = new SteamId { Value = value };
                sId = ExtendedSteamId.SetAccountInstance(sId, 393216);
                sId = ExtendedSteamId.SetAccountID(sId, value);
                sId = ExtendedSteamId.SetEAccountType(sId, EAccountType.Chat);
                id = sId;
            }
        }
        /// <summary>
        /// Is this Lobby value a valid value.
        /// This does not indicate it is a lobby simply that structurally the data is possibly a lobby
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (id == 0
                    || ExtendedSteamId.GetEAccountType(id) != EAccountType.Chat
                    || ExtendedSteamId.GetEUniverse(id) != Universe.Public)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// Get or set the lobby name
        /// </summary>
        /// <remarks>
        /// <para>
        /// The lobby name is a metadata field whoes key is "name". Setting this field will update the lobby metadata accordinly and this update will be reflected to all members.
        /// Only the owner of the lobby can set this value.
        /// </para>
        /// </remarks>
        public string Name
        {
            get => this[DataName];
            set => this[DataName] = value;
        }
        /// <summary>
        /// The current owner of the lobby.
        /// </summary>
        public LobbyMember Owner
        {
            get => new LobbyMember { lobby = this, user = API.Matchmaking.Client.GetLobbyOwner(id) };
            set => API.Matchmaking.Client.SetLobbyOwner(id, value.user);
        }
        /// <summary>
        /// The member data for this user
        /// </summary>
        public LobbyMember Me => new LobbyMember { lobby = this, user = API.User.Client.Id };
        [Obsolete("Please use Me instead.")]
        public LobbyMember User => Me;
        /// <summary>
        /// The collection of all members of this lobby including the owner of the lobby.
        /// </summary>
        public LobbyMember[] Members => API.Matchmaking.Client.GetLobbyMembers(id);
        /// <summary>
        /// True if the data type metadata is set
        /// </summary>
        public bool IsTypeSet => !string.IsNullOrEmpty(API.Matchmaking.Client.GetLobbyData(id, DataType));
        /// <summary>
        /// Returns the type of the lobby if set, if not set this will default to Private, you can check if the type is set with <see cref="IsTypeSet"/>
        /// </summary>
        public API.Matchmaking.ELobbyType Type
        {
            get
            {
                if (int.TryParse(API.Matchmaking.Client.GetLobbyData(id, DataType), out int enumVal))
                {
                    return (API.Matchmaking.ELobbyType)enumVal;
                }
                else
                    return API.Matchmaking.ELobbyType.Private;
            }
            set => API.Matchmaking.Client.SetLobbyType(id, value);
        }
        /// <summary>
        /// Gets or sets the version of the game the lobby is configured for ... this should match the owners version
        /// </summary>
        public string GameVersion
        {
            get => this[DataVersion];
            set => this[DataVersion] = value;
        }
        /// <summary>
        /// Is the user the host of this lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyOwner(SteamId)"/> and compares the results to <see cref="SteamUser.GetSteamID()"/>.
        /// This returns true if the provided lobby ID is a legitimate ID and if Valve indicates that the lobby has members and if the owner of the lobby is the current player.
        /// </para>
        /// </remarks>
        public bool IsOwner
        {
            get
            {
                return UserData.Me == ((Steamworks.Data.Lobby)this).Owner;
            }
        }
        /// <summary>
        /// Indicates rather or not this lobby is a party lobby
        /// </summary>
        public bool IsGroup
        {
            get
            {
                return this[DataMode] == "Group";
            }
            set
            {
                if (IsOwner)
                {
                    if (value)
                    {
                        SetType(API.Matchmaking.ELobbyType.Invisible);
                        this[DataMode] = "Group";
                    }
                    else
                    {
                        this[DataMode] = "General";
                    }
                }
            }
        }
        /// <summary>
        /// Does this lobby have a game server registered to it
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyGameServer(SteamId, out uint, out ushort, out SteamId)"/> and cashes the data to <see cref="GameServer"/>.
        /// It is not usually nessisary to check this value since the set game server callback from Steamworks will automatically update these values if the user was connected to the lobby when the set game server data was called.
        /// </para>
        /// </remarks>
        public bool HasServer
        {
            get
            {
                uint ip = 0;
                ushort port = 0;
                Steamworks.SteamId id = default;
                var result = ((Steamworks.Data.Lobby)this).GetGameServer(ref ip, ref port, ref id);
                return result;
            }
        }
            //SteamMatchmaking.GetLobbyGameServer(id, out _, out _, out _);
        public LobbyGameServer GameServer => API.Matchmaking.Client.GetLobbyGameServer(id);
        /// <summary>
        /// Returns true if all of the players 'IsReady' is true
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determin if the players are ready to play the game.
        /// </para>
        /// </remarks>
        public bool AllPlayersReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return Members.Any(p => !p.IsReady) ? false : true;
            }
        }
        /// <summary>
        /// Returns true if all of the players 'IsReady' is false
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determin if all players have reset the ready flag such as when some change is made after a previous ready check had already passed.
        /// </para>
        /// </remarks>
        public bool AllPlayersNotReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return Members.Any(p => p.IsReady) ? false : true;
            }
        }
        public bool IsReady
        {
            get => API.Matchmaking.Client.GetLobbyMemberData(id, API.User.Client.Id, DataReady) == "true";
            set => API.Matchmaking.Client.SetLobbyMemberData(id, DataReady, value.ToString().ToLower());
        }
        public bool Full => API.Matchmaking.Client.GetLobbyMemberLimit(id) <= ((Steamworks.Data.Lobby)this).MemberCount;
        public int MaxMembers
        {
            get => API.Matchmaking.Client.GetLobbyMemberLimit(id);
            set => API.Matchmaking.Client.SetLobbyMemberLimit(id, value);
        }
        /// <summary>
        /// Read and write metadata values to the lobby
        /// </summary>
        /// <param name="metadataKey">The key of the value to be read or writen</param>
        /// <returns>The value of the key if any otherwise returns and empty string.</returns>
        public string this[string metadataKey]
        {
            get
            {
                return API.Matchmaking.Client.GetLobbyData(id, metadataKey);
            }
            set
            {
                API.Matchmaking.Client.SetLobbyData(id, metadataKey, value);
            }
        }
        /// <summary>
        /// Get the LobbyMember object for a given user
        /// </summary>
        /// <param name="id">The ID of the member to fetch</param>
        /// <param name="member">The member found</param>
        /// <returns>True if the user is a member of the lobby, false if they are not</returns>
        public bool GetMember(SteamId id, out LobbyMember member) => API.Matchmaking.Client.GetMember(this, id, out member);
        /// <summary>
        /// Checks if a user is a member of this lobby
        /// </summary>
        /// <param name="id">The user to check for</param>
        /// <returns>True if they are, false if not</returns>
        public bool IsAMember(SteamId id) => API.Matchmaking.Client.IsAMember(this, id);
        /// <summary>
        /// Updates the lobby type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool SetType(API.Matchmaking.ELobbyType type) => API.Matchmaking.Client.SetLobbyType(id, type);
        public bool SetJoinable(bool makeJoinable) => API.Matchmaking.Client.SetLobbyJoinable(id, makeJoinable);
        /// <summary>
        /// Gets the dictionary of metadata values assigned to this lobby.
        /// </summary>
        public Dictionary<string, string> GetMetadata()
        {
            var sdLobby = (Steamworks.Data.Lobby)this;
            
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach(var kvp in sdLobby.Data)
                result.Add(kvp.Key, kvp.Value);

            return result;
        }
        /// <summary>
        /// Join this lobby
        /// </summary>
        /// <param name="callback">Handler(LobbyEnter_t result, bool IOError)</param>
        public void Join(Action<Lobby, bool> callback)
        {
            API.Matchmaking.Client.JoinLobby(this, callback);
        }

        /// <summary>
        /// Leaves the current lobby if any
        /// </summary>
        public void Leave()
        {
            if (id == 0)
                return;

            API.Matchmaking.Client.LeaveLobby(id);

            id = 0;
        }
        public bool DeleteLobbyData(string dataKey) => API.Matchmaking.Client.DeleteLobbyData(id, dataKey);

        public bool InviteUserToLobby(UserData targetUser) => API.Matchmaking.Client.InviteUserToLobby(id, targetUser);

        public bool SendChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            return ((Steamworks.Data.Lobby)this).SendChatString(message);
        }
        [System.Obsolete("You are useing Facepunch which does not lobby chat data features, if you require this feature then remove Facepunch and install Steamworks.NET")]
        public bool SendChatMessage(byte[] data)
        {
            return false;
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer(string address, ushort port, SteamId gameServerId)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, GameServer.ipAddress, port, gameServerId);
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer(string address, ushort port)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, GameServer.ipAddress, port, 0);
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer(SteamId gameServerId)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, 0, 0, gameServerId);
        }
        /// <summary>
        /// <para>
        /// This overload uses the lobby owner's SteamId as the server ID which is typical of P2P session.
        /// </para>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public void SetGameServer()
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, 0, 0, API.User.Client.Id);
        }
        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberId"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public bool KickMember(SteamId memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);

            if (kickList == null)
                kickList = string.Empty;

            if (!kickList.Contains("[" + memberId.ToString() + "]"))
                kickList += "[" + memberId.ToString() + "]";

            return API.Matchmaking.Client.SetLobbyData(id, DataKick, kickList);
        }
        public bool KickListContains(SteamId memberId)
        {
            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);
            return kickList.Contains("[" + memberId.ToString() + "]");
        }
        public bool RemoveFromKickList(SteamId memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);

            kickList = kickList.Replace("[" + memberId.ToString() + "]", string.Empty);

            return API.Matchmaking.Client.SetLobbyData(id, DataKick, kickList);
        }
        public bool ClearKickList()
        {
            if (!IsOwner)
                return false;

            return API.Matchmaking.Client.DeleteLobbyData(id, DataKick);
        }
        /// <summary>
        /// Use this sparingly it requires string parcing and is not performant
        /// </summary>
        /// <returns></returns>
        public SteamId[] GetKickList()
        {
            var list = API.Matchmaking.Client.GetLobbyData(id, DataKick);
            if (!string.IsNullOrEmpty(list))
            {
                var sArray = list.Split(new string[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                var resultList = new List<SteamId>();
                for (int i = 0; i < sArray.Length; i++)
                {
                    if (ulong.TryParse(sArray[i].Replace("[", string.Empty).Replace("]", string.Empty), out ulong id))
                        resultList.Add(new SteamId { Value = id });
                }

                return resultList.ToArray();
            }
            else
                return new SteamId[0];
        }
        /// <summary>
        /// Sets metadata for the player on the first lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetMemberMetadata(string key, string value)
        {
            API.Matchmaking.Client.SetLobbyMemberData(id, key, value);
        }
        /// <summary>
        /// Returns the metadata field of the local user
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMemberMetadata(string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, API.User.Client.Id, key);
        }
        /// <summary>
        /// Returns the metadata field of the user indicated by <paramref name="memberId"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMemberMetadata(SteamId memberId, string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, memberId, key);
        }
        /// <summary>
        /// Returns the metadata field of the user indicated by <paramref name="member"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetMemberMetadata(LobbyMember member, string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, member.user, key);
        }
        public static Lobby Get(string accountId)
        {
            if (uint.TryParse(accountId, out uint result))
                return Get(result);
            else
                return 0;
        }
        /// <summary>
        /// Get the lobby represented by this account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static Lobby Get(uint value)
        {
            var sId = new SteamId();
            sId = ExtendedSteamId.SetAccountInstance(sId, 393216);
            sId = ExtendedSteamId.SetAccountID(sId, value);
            sId = ExtendedSteamId.SetEAccountType(sId, EAccountType.Chat);
            sId = ExtendedSteamId.SetEUniverse(sId, Universe.Public);
            return sId;
        }
        /// <summary>
        /// Get the lobby represented by this SteamId value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Lobby Get(ulong id) => new Lobby { id = id };
        /// <summary>
        /// Get the lobby represented by this SteamId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Lobby Get(SteamId id) => new Lobby { id = id };
        /// <summary>
        /// Join the lobby represented by this account Id
        /// </summary>
        /// <param name="accountId">Must be a valid uint as a string</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(string accountId, Action<Lobby, bool> callback) => API.Matchmaking.Client.JoinLobby(Get(accountId), callback);
        /// <summary>
        /// Join the lobby
        /// </summary>
        /// <param name="lobby">The lobby to join</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(Lobby lobby, Action<Lobby, bool> callback) => API.Matchmaking.Client.JoinLobby(lobby, callback);
        /// <summary>
        /// Join the lobby represented by this account Id
        /// </summary>
        /// <param name="accountId">Must be a valid uint as a string</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(uint accountId, Action<Lobby, bool> callback) => API.Matchmaking.Client.JoinLobby(Get(accountId), callback);

        #region Constants
        /// <summary>
        /// Standard metadata field representing the name of the lobby.
        /// This field is typically only used in lobby metadata
        /// </summary>
        public const string DataName = "name";
        /// <summary>
        /// Heathen standard metadata field representing the version of the game.
        /// This field is commonly used in lobby and member metadata
        /// </summary>
        public const string DataVersion = "z_heathenGameVersion";
        /// <summary>
        /// Heathen standard metadata field indicating that the user is ready to play.
        /// This field is commonly only used on member metadata
        /// </summary>
        public const string DataReady = "z_heathenReady";
        /// <summary>
        /// Heathen standard metadata field indicating that these users should leave the lobby.
        /// This is a string containing each SteamId of members that should not join this lobby and if present should leave it.
        /// Data in this list is in the form of [ + SteamId + ] e.g. [123456789][987654321] would indicate 2 users that should leave
        /// This field is commonly only used on lobby metadata
        /// </summary>
        public const string DataKick = "z_heathenKick";
        /// <summary>
        /// Heathen standard metadata field indicating the mode of the lobby e.g. group or general
        /// If this is blank its assumed to be general
        /// </summary>
        public const string DataMode = "z_heathenMode";
        /// <summary>
        /// Heathen standard metadata field indicating the type of lobby e.g. private, friend, public or invisible
        /// </summary>
        public const string DataType = "z_heathenType";
        #endregion

        #region Boilerplate
        public int CompareTo(SteamId other)
        {
            return id.Value.CompareTo(other.Value);
        }

        public int CompareTo(ulong other)
        {
            return id.Value.CompareTo(other);
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public bool Equals(SteamId other)
        {
            return id.Equals(other);
        }

        public bool Equals(ulong other)
        {
            return id.Value.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(Lobby l, Lobby r) => l.id == r.id;
        public static bool operator ==(SteamId l, Lobby r) => l == r.id;
        public static bool operator ==(Lobby l, SteamId r) => l.id == r;
        public static bool operator ==(Lobby l, ulong r) => l.id.Value == r;
        public static bool operator ==(ulong l, Lobby r) => l == r.id.Value;
        public static bool operator !=(Lobby l, Lobby r) => l.id != r.id;
        public static bool operator !=(SteamId l, Lobby r) => l != r.id;
        public static bool operator !=(Lobby l, SteamId r) => l.id != r;
        public static bool operator !=(Lobby l, ulong r) => l.id.Value != r;
        public static bool operator !=(ulong l, Lobby r) => l != r.id.Value;

        public static implicit operator SteamId(Lobby c) => c.id;
        public static implicit operator Steamworks.Data.Lobby(Lobby c) => new Steamworks.Data.Lobby(c.id);
        public static implicit operator Lobby(SteamId id) => new Lobby { id = id };
        public static implicit operator Lobby(Steamworks.Data.Lobby id) => new Lobby { id = id.Id };
        public static implicit operator ulong(Lobby id) => id.id.Value;
        public static implicit operator Lobby(ulong id) => new Lobby { id = new SteamId { Value = id } };

        #endregion
    }
#endif

}
#endif