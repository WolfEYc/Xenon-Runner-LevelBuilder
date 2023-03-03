using System.Collections;
using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using UnityEngine;

namespace SpeedrunSim
{
    public class LeaderboardDisplay : StaticInstance<LeaderboardDisplay>
    {
        public SteamLeaderboard_t leaderboardId;
        public LeaderboardManager leaderboardManager;
        [SerializeField] LeaderboardEntryRecord myRecord;
        [SerializeField] LeaderboardEntryRecord[] topTenRecords;
        [SerializeField] LeaderboardEntryRecord[] friendRecords;
        [field: SerializeField] public GhostRunner GhostRunner { get; private set; }
        [SerializeField] Canvas worldCanvas;
        
        IEnumerator Start()
        {
            worldCanvas.worldCamera = Camera.main;

            leaderboardManager.leaderboard.leaderboardId = leaderboardId;
            
            yield return new WaitUntil(() => SteamSettings.Initialized);

            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            leaderboardManager.GetAllFriendsEntries();
            leaderboardManager.RefreshUserEntry();
            leaderboardManager.GetTopEntries(10);
            
           
            Debug.Log($"Refreshing Leaderboard Display with ID: {leaderboardManager.leaderboard.leaderboardId}");
        }

        public void HandleBoardQuery(LeaderboardEntry[] entries)
        {
            Debug.Log($"Received {entries.Length} Global LeaderboardEntries");
            
            int i;
            for (i = 0; i < entries.Length; i++)
            {
                topTenRecords[i].SetEntry(entries[i]);
            }

            for (; i < topTenRecords.Length; i++)
            {
                topTenRecords[i].SetEntry(null);
            }
        }
        
        public void HandleFriendQuery(LeaderboardEntry[] entries)
        {
            
            int i;
            for (i = 0; i < entries.Length; i++)
            {
                friendRecords[i].SetEntry(entries[i]);
            }
            for (; i < friendRecords.Length; i++)
            {
                friendRecords[i].SetEntry(null);
            }
        }
        
        public void HandleSelfUpdate(LeaderboardEntry self)
        {
            myRecord.SetEntry(self);
            
        }

        public void SelfFindFailed()
        {
            Debug.LogError("Failed To Find Score!");
        }
        
        public void SelfUploadFailed()
        {
            Debug.LogError("Failed To Upload Score!");
        }
    }
}
