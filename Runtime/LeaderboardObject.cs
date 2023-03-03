#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// <para>Represents a Steamworks Leaderboard and manages its entries and quries</para>
    /// <para>To create a new <see cref="LeaderboardObject"/> object in your project right click in a folder in your project and select</para>
    /// <para>Create >> Steamworks >> Player Services >> Leaderboard Data</para>
    /// </summary>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/leaderboard-object")]
    [CreateAssetMenu(menuName = "Steamworks/Leaderboard Object")]
#if STEAMWORKSNET
    public class LeaderboardObject : ScriptableObject
    {
        /// <summary>
        /// Should the board be created if missing on the target app
        /// </summary>
        [HideInInspector]
        public bool createIfMissing;
        /// <summary>
        /// If creating a board what sort method should be applied
        /// </summary>
        public ELeaderboardSortMethod sortMethod = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending;
        /// <summary>
        /// If creating a board what display type is it
        /// </summary>
        public ELeaderboardDisplayType displayType = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric;
        /// <summary>
        /// What is the name of the board ... if this is not to be created at run time then this must match the name as it appears in Steamworks
        /// </summary>
        [HideInInspector]
        public string leaderboardName;
        /// <summary>
        /// How many detail entries should be allowed on entries from this board
        /// </summary>
        [HideInInspector]
        public int maxDetailEntries = 0;
        /// <summary>
        /// What is the leaderboard ID ... this is nullable if null then no leaderboard has been connected
        /// </summary>
        
        public SteamLeaderboard_t leaderboardId;

        public bool Valid => leaderboardId.m_SteamLeaderboard > 0;
        public int EntryCount => API.Leaderboards.Client.GetEntryCount(leaderboardId);

        /// <summary>
        /// Returns the user entry for the local user
        /// </summary>
        /// <param name="callback">The deligate to invoke when the process is complete</param>
        public void GetUserEntry(Action<LeaderboardEntry, bool> callback)
        {
            API.Leaderboards.Client.DownloadEntries(leaderboardId, new CSteamID[] { API.User.Client.Id }, maxDetailEntries, (results, error) =>
            {
                if (error || results.Length == 0)
                    callback.Invoke(null, error);
                else
                    callback.Invoke(results[0], error);
            });
        }
        /// <summary>
        /// Invokes the callback with the query results
        /// </summary>
        /// <param name="request">The type of range to get from the board</param>
        /// <param name="start">The index to start downloading at</param>
        /// <param name="end">The index to end downloading at</param>
        /// <param name="callback">The deligate to invoke when the process is complete</param>
        public void GetEntries(ELeaderboardDataRequest request, int start, int end, Action<LeaderboardEntry[], bool> callback) => API.Leaderboards.Client.DownloadEntries(leaderboardId, request, start, end, maxDetailEntries, callback);
        /// <summary>
        /// Invokes the callback with the query results 
        /// </summary>
        /// <param name="users">The users to get results for</param>
        /// <param name="callback">The deligate to invoke when the process is complete</param>
        public void GetEntries(UserData[] users, Action<LeaderboardEntry[], bool> callback) => API.Leaderboards.Client.DownloadEntries(leaderboardId, Array.ConvertAll<UserData, CSteamID>(users, p => p.id), maxDetailEntries, callback);
        /// <summary>
        /// Invokes the callback with the query results 
        /// </summary>
        /// <param name="users">The users to get results for</param>
        /// <param name="callback">The deligate to invoke when the process is complete</param>
        public void GetEntries(CSteamID[] users, Action<LeaderboardEntry[], bool> callback) => API.Leaderboards.Client.DownloadEntries(leaderboardId, users, maxDetailEntries, callback);
        /// <summary>
        /// Registers the board on Steamworks creating if configured to do so or locating if not.
        /// </summary>
        public void Register()
        {
            if (createIfMissing)
                FindOrCreateLeaderboard();
            else
                FindLeaderboard();
        }

        private void FindOrCreateLeaderboard()
        {
            if(displayType == ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone)
            {
                Debug.LogError("Display Type none is not a valid display type. No board will be created");
                return;
            }

            if(sortMethod == ELeaderboardSortMethod.k_ELeaderboardSortMethodNone)
            {
                Debug.LogError("Sort Method none is not a valid type. No board will be created");
                return;
            }

            API.Leaderboards.Client.FindOrCreate(leaderboardName, sortMethod, displayType, (r, e) =>
            {
                if (r.m_bLeaderboardFound == 0 || e)
                {
                    Debug.LogError("Failed to find or create leaderboard", this);
                    return;
                }

                if (r.m_bLeaderboardFound != 0)
                {
                    leaderboardId = r.m_hSteamLeaderboard;
                }
            });
        }

        private void FindLeaderboard()
        {
            API.Leaderboards.Client.Find(leaderboardName, (r, e) =>
            {
                if (r.m_bLeaderboardFound == 0 || e)
                {
                    Debug.LogError("Failed to find leaderboard", this);
                    return;
                }

                if (r.m_bLeaderboardFound != 0)
                {
                    leaderboardId = r.m_hSteamLeaderboard;
                }
            });
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">{ LeaderboardScoreUploaded_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure.</param>
        public void UploadScore(int score, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded_t, bool> callback = null) => API.Leaderboards.Client.UploadScore(leaderboardId, method, score, null, callback);

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">{ LeaderboardScoreUploaded_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure.</param>
        public void UploadScore(int score, int[] scoreDetails, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded_t, bool> callback = null) => API.Leaderboards.Client.UploadScore(leaderboardId, method, score, scoreDetails, callback);

        /// <summary>
        /// Attempts to save, share and attach an object to the leaderboard
        /// </summary>
        /// <remarks>
        /// Note that this depends on being able to save the file to the User's Remote Storage which is a limited resoruce so use this sparingly.
        /// </remarks>
        /// <param name="fileName">The name the file should be saved as. This must be unique on the user's storage</param>
        /// <param name="JsonObject">A JsonUtility serialisable object, we will serialize this to UTF8 format and then convert to byte[] for you and upload to Steam Remote Storage</param>
        /// <param name="callback">{ LeaderbaordUGCSet_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure from Valve.</param>
        public void AttachUGC(string fileName, object jsonObject, System.Text.Encoding encoding, Action<LeaderboardUGCSet_t, bool> callback = null) => API.Leaderboards.Client.AttachUGC(leaderboardId, fileName, jsonObject, encoding, callback);
    }
#elif FACEPUNCH
    public class LeaderboardObject : ScriptableObject
    {
        /// <summary>
        /// Should the board be created if missing on the target app
        /// </summary>
        [HideInInspector]
        public bool createIfMissing;
        /// <summary>
        /// If creating a board what sort method should be applied
        /// </summary>
        public Steamworks.Data.LeaderboardSort sortMethod = Steamworks.Data.LeaderboardSort.Ascending;
        /// <summary>
        /// If creating a board what display type is it
        /// </summary>
        public Steamworks.Data.LeaderboardDisplay displayType = Steamworks.Data.LeaderboardDisplay.Numeric;
        /// <summary>
        /// What is the name of the board ... if this is not to be created at run time then this must match the name as it appears in Steamworks
        /// </summary>
        [HideInInspector]
        public string leaderboardName;
        /// <summary>
        /// How many detail entries should be allowed on entries from this board
        /// </summary>
        [HideInInspector]
        public int maxDetailEntries = 0;
        /// <summary>
        /// What is the leaderboard ID ... this is nullable if null then no leaderboard has been connected
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public Steamworks.Data.Leaderboard leaderboardId;
        [Obsolete("Facepunch decided to hid the leaderboard ID ... apparently that is to much for you to handle, so we cant tell if the ID is valid or not")]
        public bool Valid => true;
        public int EntryCount => API.Leaderboards.Client.GetEntryCount(leaderboardId);

        /// <summary>
        /// Returns the user entry for the local user
        /// </summary>
        /// <param name="callback">The deligate to invoke when the process is complete</param>
        public void GetUserEntry(Action<Steamworks.Data.LeaderboardEntry?> callback)
        {
            API.Leaderboards.Client.DownloadEntries(leaderboardId, API.Leaderboards.ELeaderboardDataRequest.GlobalAroundUser, 0, 1, (results) =>
            {
                if(results.Length > 0)
                {
                    foreach (var entry in results)
                        if (entry.User.Id == UserData.Me.id)
                        {
                            callback?.Invoke(entry);
                            return;
                        }
                }
                callback?.Invoke(null);
            });
        }
        /// <summary>
        /// Invokes the callback with the query results
        /// </summary>
        /// <param name="request">The type of range to get from the board</param>
        /// <param name="start">The index to start downloading at</param>
        /// <param name="end">The index to end downloading at</param>
        /// <param name="callback">The deligate to invoke when the process is complete</param>
        public void GetEntries(API.Leaderboards.ELeaderboardDataRequest request, int start, int end, Action<Steamworks.Data.LeaderboardEntry[]> callback) => API.Leaderboards.Client.DownloadEntries(leaderboardId, request, start, end, callback);
        /// <summary>
        /// Registers the board on Steamworks creating if configured to do so or locating if not.
        /// </summary>
        public void Register()
        {
            if (createIfMissing)
                FindOrCreateLeaderboard();
            else
                FindLeaderboard();
        }

        private void FindOrCreateLeaderboard()
        {
            API.Leaderboards.Client.FindOrCreate(leaderboardName, sortMethod, displayType, (r) =>
            {
                if (!r.HasValue)
                {
                    Debug.LogError("Failed to find or create leaderboard", this);
                    return;
                }

                if (r.HasValue)
                {
                    leaderboardId = r.Value;
                }
            });
        }

        private void FindLeaderboard()
        {
            API.Leaderboards.Client.Find(leaderboardName, (r) =>
            {
                if (!r.HasValue)
                {
                    Debug.LogError("Failed to find leaderboard", this);
                    return;
                }

                if (r.HasValue)
                {
                    leaderboardId = r.Value;
                }
            });
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">{ LeaderboardScoreUploaded_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure.</param>
        public void UploadScore(int score, int[] details, Action<Steamworks.Data.LeaderboardUpdate?> callback = null) => API.Leaderboards.Client.UploadScore(leaderboardId, score, details, callback);

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">{ LeaderboardScoreUploaded_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure.</param>
        public void ReplaceScore(int score, int[] details, Action<Steamworks.Data.LeaderboardUpdate?> callback = null) => API.Leaderboards.Client.ReplaceScore(leaderboardId, score, details, callback);
    }
#endif

}
#endif