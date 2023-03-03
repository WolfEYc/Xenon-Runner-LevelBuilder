using HeathenEngineering.SteamworksIntegration.UI;
using TMPro;
using UnityEngine;

namespace SpeedrunSim
{
    public class MainLeaderboardEntryDisplay : MonoBehaviour, ILeaderboardEntryInGame
    {
        [SerializeField] TMP_Text rank, userName, time;
        [SerializeField] SetUserAvatar userImage;
        
        public void Display(LeaderboardEntryRecord record)
        {
            var entry = record.Entry;

            if (entry == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            userImage.LoadAvatar(entry.User);
            userName.SetText(entry.User.Nickname);
            rank.SetText(entry.Rank.ToString());
            time.SetText(entry.Score.TimeStringFromMS());
            gameObject.SetActive(true);
        }
    }
}
