#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    public class LeaderboardManager : MonoBehaviour
    {
        public LeaderboardObject leaderboard;

        private LeaderboardEntry _lastKnownUserEntry;
        public LeaderboardEntry LastKnownUserEntry
        {
            get => _lastKnownUserEntry;
            private set
            {
                _lastKnownUserEntry = value;
                evtUserEntryUpdated.Invoke(value);
            }
        }

        public UserEntryEvent evtUserEntryUpdated;
        public EntryResultsEvent evtQueryCompleted;
        public EntryResultsEvent evtFriendsQueryCompleted;
        
        public UnityEvent evtQueryError;
        public UnityEvent evtUploadError;
        
        public void RefreshUserEntry()
        {
            leaderboard.GetUserEntry((r, e) =>
            {
                if (!e)
                    LastKnownUserEntry = r;
                else
                    evtQueryError.Invoke();
            });
        }

        public void GetTopEntries(int count)
        {
            leaderboard.GetEntries(Steamworks.ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, count, (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void GetNearbyEntries(int beforeUser, int afterUser)
        {
            leaderboard.GetEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -beforeUser, afterUser, (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void GetAllFriendsEntries()
        {
            leaderboard.GetEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends, 0, 10, (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtFriendsQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void GetUserEntries(IEnumerable<UserData> users)
        {
            leaderboard.GetEntries(users.ToArray(), (r, e) =>
            {
                if (!e)
                {
                    var user = r.FirstOrDefault(p => p.entry.m_steamIDUser == Steamworks.SteamUser.GetSteamID());
                    if (user != default)
                        LastKnownUserEntry = user;

                    evtQueryCompleted.Invoke(r);
                }
                else
                    evtQueryError?.Invoke();
            });
        }

        public void UploadScore(int score)
        {
            leaderboard.UploadScore(score, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (r, e) =>
            {
                if (!e)
                {
                    //RefreshUserEntry();
                }
                else
                    evtUploadError.Invoke();
            });
        }

        public void UploadScore(int score, int[] details)
        {
            leaderboard.UploadScore(score, details, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (r, e) =>
            {
                if (!e)
                {
                    //RefreshUserEntry();
                }
                else
                    evtUploadError.Invoke();
            });
        }

        public void ForceScore(int score, Action<LeaderboardScoreUploaded_t,bool> callback = null)
        {
            leaderboard.UploadScore(score, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, callback);
        }

        public void ForceScore(int score, int[] details)
        {
            leaderboard.UploadScore(score, details, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, (r, e) =>
            {
                if (!e)
                {
                    //RefreshUserEntry();
                }
                else
                    evtUploadError.Invoke();
            });
        }

        [Serializable]
        public class UserEntryEvent : UnityEvent<LeaderboardEntry> { }
        [Serializable]
        public class EntryResultsEvent : UnityEvent<LeaderboardEntry[]> { }
    }
}
#endif