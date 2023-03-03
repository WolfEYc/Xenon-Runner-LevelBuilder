#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents the in game definition of a Steamworks DLC app.
    /// </summary>
    /// <remarks>Steamworks DLC or Downloadable Content is defined on the Steamworks API in your Steamworks Portal.
    /// Please carfully read <a href="https://partner.steamgames.com/doc/store/application/dlc">https://partner.steamgames.com/doc/store/application/dlc</a> before designing features are this concept.</remarks>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/downloadable-content-object")]
    [CreateAssetMenu(menuName = "Steamworks/Downloadable Content Object")]
    public class DownloadableContentObject : ScriptableObject
    {
        public AppId_t AppId
        {
            get => appId;
#if UNITY_EDITOR
            set => appId = value;
#endif
        }

        /// <summary>
        /// The <see cref="AppId_t"/> assoceated with this DLC
        /// </summary>
        [SerializeField]
        private AppId_t appId;
        /// <summary>
        /// Is the current user 'subscribed' to this DLC.
        /// This indicates that the current user has right/license this DLC or not.
        /// </summary>
        public bool IsSubscribed
        {
            get
            {
                if (appId == default)
                    Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

                return SteamApps.BIsSubscribedApp(appId);
            }
        }
        /// <summary>
        /// Is this DLC currently installed.
        /// </summary>
        public bool IsInstalled
        {
            get
            {
                if (appId == default)
                    Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

                return SteamApps.BIsDlcInstalled(appId);
            }
        }
        
        /// <summary>
        /// Returns the install location of the DLC
        /// </summary>
        /// <returns></returns>
        public string GetInstallDirectory()
        {
            if (appId == default)
                Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

            string path;
            if(SteamApps.GetAppInstallDir(appId, out path, 2048) > 0)
            {
                return path;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Updates the IsDownloading member and Returns the download progress of the DLC if any
        /// </summary>
        /// <returns></returns>
        public float GetDownloadProgress()
        {
            if (appId == default)
                Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

            ulong current;
            ulong total;
            var IsDownloading = SteamApps.GetDlcDownloadProgress(appId, out current, out total);
            if (IsDownloading)
            {
                return Convert.ToSingle(current / (double)total);
            }
            else
                return 0f;
        }

        /// <summary>
        /// Gets the time of purchase
        /// </summary>
        /// <returns></returns>
        public DateTime GetEarliestPurchaseTime()
        {
            if (appId == default)
                Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

            var val = SteamApps.GetEarliestPurchaseUnixTime(appId);
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(val);
            return dateTime;
        }

        /// <summary>
        /// Installs the DLC
        /// </summary>
        public void Install()
        {
            if (appId == default)
                Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

            SteamApps.InstallDLC(appId);
        }

        /// <summary>
        /// Uninstalls the DLC
        /// </summary>
        public void Uninstall()
        {
            if (appId == default)
                Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

            SteamApps.UninstallDLC(appId);
        }

        /// <summary>
        /// Opens the store page to the DLC
        /// </summary>
        /// <param name="flag"></param>
        public void OpenStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None)
        {
            if (appId == default)
                Debug.Log("The app ID for DLC " + name + " is empty, this is not valid and sugests an issue with your configuraiton.");

            SteamFriends.ActivateGameOverlayToStore(appId, flag);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(DownloadableContentObject))]
    public class DownloadContentObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var dlc = target as DownloadableContentObject;
            UnityEditor.EditorGUILayout.SelectableLabel("App ID: " + dlc.AppId.ToString());
        }
    }
#endif
}
#endif