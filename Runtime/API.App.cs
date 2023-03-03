#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace HeathenEngineering.SteamworksIntegration.API
{
    /// <summary>
    /// Exposes a wide range of information and actions for applications and Downloadable Content (DLC).
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/ISteamApps
    /// </remarks>
    public static class App
    {
#if STEAMWORKSNET
        public static AppId_t Id => SteamUtils.GetAppID();
        public static class Client
        {
            [Serializable]
            public class UnityEventServersDisconnected : UnityEvent<EResult>
            { }

            [Serializable]
            public class UnityEventServersConnectFailure : UnityEvent<SteamServerConnectFailure_t>
            { }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                m_FileDetailResult_t = null;
                m_DlcInstalled_t = null;
                m_NewUrlLaunchParameters_t = null;

                eventDlcInstalled = new DlcInstalledEvent();
                eventNewUrlLaunchParameters = new NewUrlLaunchParametersEvent();
                eventServersConnected = new UnityEvent();
                eventServersDisconnected = new UnityEventServersDisconnected();
            }

            /// <summary>
            /// Triggered after the current user gains ownership of DLC and that DLC is installed.
            /// </summary>
            public static DlcInstalledEvent EventDlcInstalled
            { 
                get
                {
                    if (m_DlcInstalled_t == null)
                        m_DlcInstalled_t = Callback<DlcInstalled_t>.Create(eventDlcInstalled.Invoke);

                    return eventDlcInstalled;
                }
            }

            /// <summary>
            /// Posted after the user executes a steam url with command line or query parameters such as steam://run/<appid>//?param1=value1;param2=value2;param3=value3; while the game is already running. The new params can be queried with GetLaunchCommandLine and GetLaunchQueryParam.
            /// </summary>
            public static NewUrlLaunchParametersEvent EventNewUrlLaunchParameters
            {
                get
                {
                    if (m_NewUrlLaunchParameters_t == null)
                        m_NewUrlLaunchParameters_t = Callback<NewUrlLaunchParameters_t>.Create(eventNewUrlLaunchParameters.Invoke);

                    return eventNewUrlLaunchParameters;
                }
            }

            public static UnityEvent EventServersConnected
            {
                get
                {
                    if (m_SteamServersConnected_t == null)
                        m_SteamServersConnected_t = Callback<SteamServersConnected_t>.Create((connected) =>
                        {
                            eventServersConnected?.Invoke();
                        });

                    return eventServersConnected;
                }
            }

            public static UnityEventServersDisconnected EventServersDisconnected
            {
                get
                {
                    if (m_SteamServersConnected_t == null)
                        m_SteamServersDisconnected_t = Callback<SteamServersDisconnected_t>.Create((connected) =>
                        {
                            eventServersDisconnected?.Invoke(connected.m_eResult);
                        });

                    return eventServersDisconnected;
                }
            }

            public static UnityEventServersConnectFailure EventServersConnectFailure
            {
                get
                {
                    if (m_SteamServerConnectFailure_t == null)
                        m_SteamServerConnectFailure_t = Callback<SteamServerConnectFailure_t>.Create((connected) =>
                        {
                            eventServersConnectFailure?.Invoke(connected);
                        });

                    return eventServersConnectFailure;
                }
            }

            private static DlcInstalledEvent eventDlcInstalled = new DlcInstalledEvent();
            private static NewUrlLaunchParametersEvent eventNewUrlLaunchParameters = new NewUrlLaunchParametersEvent();
            private static UnityEvent eventServersConnected = new UnityEvent();
            private static UnityEventServersDisconnected eventServersDisconnected = new UnityEventServersDisconnected();
            private static UnityEventServersConnectFailure eventServersConnectFailure = new UnityEventServersConnectFailure();

            private static CallResult<FileDetailsResult_t> m_FileDetailResult_t;
            private static Callback<DlcInstalled_t> m_DlcInstalled_t;
            private static Callback<NewUrlLaunchParameters_t> m_NewUrlLaunchParameters_t;
            private static Callback<SteamServerConnectFailure_t> m_SteamServerConnectFailure_t;
            private static Callback<SteamServersConnected_t> m_SteamServersConnected_t;
            private static Callback<SteamServersDisconnected_t> m_SteamServersDisconnected_t;

            /// <summary>
            /// Checks if the active user is subscribed to the current App ID.
            /// </summary>
            /// <remarks>
            /// NOTE: This will always return true if you're using Steam DRM or calling SteamAPI_RestartAppIfNecessary.
            /// </remarks>
            public static bool IsSubscribed => SteamApps.BIsSubscribed();
            /// <summary>
            /// Checks if the active user is accessing the current appID via a temporary Family Shared license owned by another user.
            /// </summary>
            public static bool IsSubscribedFromFamilySharing => SteamApps.BIsSubscribedFromFamilySharing();
            /// <summary>
            /// Checks if the user is subscribed to the current App ID through a free weekend.
            /// </summary>
            public static bool IsSubscribedFromFreeWeekend => SteamApps.BIsSubscribedFromFreeWeekend();
            /// <summary>
            /// Checks if the user has a VAC ban on their account
            /// </summary>
            public static bool IsVACBanned => SteamApps.BIsVACBanned();
            /// <summary>
            /// Gets the Steam ID of the original owner of the current app. If it's different from the current user then it is borrowed.
            /// </summary>
            public static UserData Owner => SteamApps.GetAppOwner();
            /// <summary>
            /// Returns a list of languages supported by the app
            /// </summary>
            public static string[] AvailableLanguages
            {
                get
                {
                    var list = SteamApps.GetAvailableGameLanguages();
                    return list.Split(',');
                }
            }
            /// <summary>
            /// Returns true if a beta branch is being used
            /// </summary>
            public static bool IsBeta => SteamApps.GetCurrentBetaName(out string _, 128);
            /// <summary>
            /// Returns the name of the beta branch being used if any
            /// </summary>
            public static string CurrentBetaName
            {
                get
                {
                    if (SteamApps.GetCurrentBetaName(out string name, 512))
                        return name;
                    else
                        return string.Empty;
                }
            }
            /// <summary>
            /// Gets the current language that the user has set
            /// </summary>
            public static string CurrentGameLanguage => SteamApps.GetCurrentGameLanguage();
            /// <summary>
            /// Returns the metadata for all available DLC
            /// </summary>
            public static DlcData[] Dlc
            {
                get
                {
                    var count = SteamApps.GetDLCCount();
                    if (count > 0)
                    {
                        var result = new DlcData[count];
                        for (int i = 0; i < count; i++)
                        {
                            if (SteamApps.BGetDLCDataByIndex(i, out AppId_t appid, out bool available, out string name, 512))
                            {
                                result[i] = new DlcData(appid, available, name);
                            }
                            else
                            {
                                Debug.LogWarning("Failed to fetch DLC at index [" + i.ToString() + "]");
                            }
                        }
                        return result;
                    }
                    else
                        return new DlcData[0];
                }
            }
            /// <summary>
            /// Checks whether the current App ID is for Cyber Cafes.
            /// </summary>
            public static bool IsCybercafe => SteamApps.BIsCybercafe();
            /// <summary>
            /// Checks if the license owned by the user provides low violence depots.
            /// </summary>
            public static bool IsLowViolence => SteamApps.BIsLowViolence();
            /// <summary>
            /// Gets the App ID of the current process.
            /// </summary>
            public static AppId_t Id => SteamUtils.GetAppID();
            /// <summary>
            /// Gets the buildid of this app, may change at any time based on backend updates to the game.
            /// </summary>
            public static int BuildId => SteamApps.GetAppBuildId();
            /// <summary>
            /// Gets the install folder for a specific AppID.
            /// </summary>
            public static string InstallDirectory
            {
                get
                {
                    SteamApps.GetAppInstallDir(SteamUtils.GetAppID(), out string folder, 2048);
                    return folder;
                }
            }
            /// <summary>
            /// Gets the number of DLC pieces for the current app.
            /// </summary>
            public static int DLCCount => SteamApps.GetDLCCount();
            /// <summary>
            /// Gets the command line if the game was launched via Steam URL, e.g. steam://run/&lt;appid&gt;//&lt;command line&gt;/. This method is preferable to launching with a command line via the operating system, which can be a security risk. In order for rich presence joins to go through this and not be placed on the OS command line, you must enable "Use launch command line" from the Installation &gt; General page on your app.
            /// </summary>
            public static string LaunchCommandLine
            {
                get
                {
                    if (
                SteamApps.GetLaunchCommandLine(out string commandline, 512) > 0)
                        return commandline;
                    else
                        return string.Empty;
                }
            }

            /// <summary>
            /// Checks if a specific app is installed.
            /// </summary>
            /// <remarks>
            /// The app may not actually be owned by the current user, they may have it left over from a free weekend, etc.
            /// This only works for base applications, not Downloadable Content(DLC). Use IsDlcInstalled for DLC instead.
            /// </remarks>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static bool IsAppInstalled(AppId_t appId) => SteamApps.BIsAppInstalled(appId);
            /// <summary>
            /// Checks if the user owns a specific DLC and if the DLC is installed
            /// </summary>
            /// <param name="appId">The App ID of the DLC to check.</param>
            /// <returns></returns>
            public static bool IsDlcInstalled(AppId_t appId) => SteamApps.BIsDlcInstalled(appId);
            /// <summary>
            /// Gets the download progress for optional DLC.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="bytesDownloaded"></param>
            /// <param name="bytesTotal"></param>
            /// <returns></returns>
            public static bool GetDlcDownloadProgress(AppId_t appId, out ulong bytesDownloaded, out ulong bytesTotal) => SteamApps.GetDlcDownloadProgress(appId, out bytesDownloaded, out bytesTotal);
            /// <summary>
            /// Gets the install directory of the app if any
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static string GetAppInstallDirectory(AppId_t appId)
            {
                SteamApps.GetAppInstallDir(appId, out string folder, 2048);
                return folder;
            }
            /// <summary>
            /// Returns the collection of installed depots in mount order
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static DepotId_t[] InstalledDepots(AppId_t appId)
            {
                var results = new DepotId_t[256];
                var count = SteamApps.GetInstalledDepots(appId, results, 256);
                Array.Resize(ref results, (int)count);
                return results;
            }
            /// <summary>
            /// Parameter names starting with the character '@' are reserved for internal use and will always return an empty string. Parameter names starting with an underscore '_' are reserved for steam features -- they can be queried by the game, but it is advised that you not param names beginning with an underscore for your own features.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static string QueryLaunchParam(string key) => SteamApps.GetLaunchQueryParam(key);
            /// <summary>
            /// Install an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void InstallDLC(AppId_t appId) => SteamApps.InstallDLC(appId);
            /// <summary>
            /// Uninstall an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void UninstallDLC(AppId_t appId) => SteamApps.UninstallDLC(appId);
            /// <summary>
            /// Checks if the active user is subscribed to a specified appId.
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static bool IsSubscribedApp(AppId_t appId) => SteamApps.BIsSubscribedApp(appId);
            public static bool IsTimedTrial(out uint secondsAllowed, out uint secondsPlayed) => SteamApps.BIsTimedTrial(out secondsAllowed, out secondsPlayed);
            /// <summary>
            /// Gets the current beta branch name if any
            /// </summary>
            /// <param name="name">outputs the name of the current beta branch if any</param>
            /// <returns>True if the user is running from a beta branch</returns>
            public static bool GetCurrentBetaName(out string name) => SteamApps.GetCurrentBetaName(out name, 512);
            /// <summary>
            /// Gets the time of purchase of the specified app
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static DateTime GetEarliestPurchaseTime(AppId_t appId)
            {
                var secondsSince1970 = SteamApps.GetEarliestPurchaseUnixTime(appId);
                return new DateTime(1970, 1, 1).AddSeconds(secondsSince1970);
            }
            /// <summary>
            /// Asynchronously retrieves metadata details about a specific file in the depot manifest.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="callback"></param>
            public static void GetFileDetails(string name, Action<FileDetailsResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_FileDetailResult_t == null)
                    m_FileDetailResult_t = CallResult<FileDetailsResult_t>.Create();

                var handle = SteamApps.GetFileDetails(name);
                m_FileDetailResult_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// If you detect the game is out-of-date (for example, by having the client detect a version mismatch with a server), you can call use MarkContentCorrupt to force a verify, show a message to the user, and then quit.
            /// </summary>
            /// <param name="missingFilesOnly"></param>
            /// <returns></returns>
            public static bool MarkContentCorrupt(bool missingFilesOnly) => SteamApps.MarkContentCorrupt(missingFilesOnly);

        }
#elif FACEPUNCH
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                SteamApps.OnDlcInstalled -= eventDlcInstalled.Invoke;
                SteamApps.OnNewLaunchParameters -= eventNewUrlLaunchParameters.Invoke;

                eventDlcInstalled = new DlcInstalledEvent();
                eventNewUrlLaunchParameters = new NewUrlLaunchParametersEvent();

            }



            /// <summary>
            /// Triggered after the current user gains ownership of DLC and that DLC is installed.
            /// </summary>
            public static DlcInstalledEvent EventDlcInstalled
            {
                get
                {
                    SteamApps.OnDlcInstalled -= eventDlcInstalled.Invoke;
                    SteamApps.OnDlcInstalled += eventDlcInstalled.Invoke;

                    return eventDlcInstalled;
                }
            }

            /// <summary>
            /// Posted after the user executes a steam url with command line or query parameters such as steam://run/<appid>//?param1=value1;param2=value2;param3=value3; while the game is already running. The new params can be queried with GetLaunchCommandLine and GetLaunchQueryParam.
            /// </summary>
            public static NewUrlLaunchParametersEvent EventNewUrlLaunchParameters
            {
                get
                {
                    SteamApps.OnNewLaunchParameters -= eventNewUrlLaunchParameters.Invoke;
                    SteamApps.OnNewLaunchParameters += eventNewUrlLaunchParameters.Invoke;

                    return eventNewUrlLaunchParameters;
                }
            }

            private static DlcInstalledEvent eventDlcInstalled = new DlcInstalledEvent();
            private static NewUrlLaunchParametersEvent eventNewUrlLaunchParameters = new NewUrlLaunchParametersEvent();

            /// <summary>
            /// Checks if the active user is subscribed to the current App ID.
            /// </summary>
            /// <remarks>
            /// NOTE: This will always return true if you're using Steam DRM or calling SteamAPI_RestartAppIfNecessary.
            /// </remarks>
            public static bool IsSubscribed => SteamApps.IsSubscribed;
            /// <summary>
            /// Checks if the active user is accessing the current appID via a temporary Family Shared license owned by another user.
            /// </summary>
            public static bool IsSubscribedFromFamilySharing => SteamApps.IsSubscribedFromFamilySharing;
            /// <summary>
            /// Checks if the user is subscribed to the current App ID through a free weekend.
            /// </summary>
            public static bool IsSubscribedFromFreeWeekend => SteamApps.IsSubscribedFromFreeWeekend;
            /// <summary>
            /// Checks if the user has a VAC ban on their account
            /// </summary>
            public static bool IsVACBanned => SteamApps.IsVACBanned;
            /// <summary>
            /// Gets the Steam ID of the original owner of the current app. If it's different from the current user then it is borrowed.
            /// </summary>
            public static UserData Owner => SteamApps.AppOwner;
            /// <summary>
            /// Returns a list of languages supported by the app
            /// </summary>
            public static string[] AvailableLanguages => SteamApps.AvailableLanguages;
            /// <summary>
            /// Returns true if a beta branch is being used
            /// </summary>
            public static bool IsBeta => SteamApps.CurrentBetaName != null;
            /// <summary>
            /// Returns the name of the beta branch being used if any
            /// </summary>
            public static string CurrentBetaName => SteamApps.CurrentBetaName;
            /// <summary>
            /// Gets the current language that the user has set
            /// </summary>
            public static string CurrentGameLanguage => SteamApps.GameLanguage;
            /// <summary>
            /// Returns the metadata for all available DLC
            /// </summary>
            public static DlcData[] Dlc
            {
                get
                {
                    var dlcInfo = SteamApps.DlcInformation().ToArray();


                    var count = dlcInfo.Length;
                    if (count > 0)
                    {
                        var result = new DlcData[count];
                        for (int i = 0; i < count; i++)
                        {
                                result[i] = dlcInfo[i];
                        }
                        return result;
                    }
                    else
                        return new DlcData[0];
                }
            }
            /// <summary>
            /// Checks whether the current App ID is for Cyber Cafes.
            /// </summary>
            public static bool IsCybercafe => SteamApps.IsCybercafe;
            /// <summary>
            /// Checks if the license owned by the user provides low violence depots.
            /// </summary>
            public static bool IsLowViolence => SteamApps.IsLowVoilence;
            /// <summary>
            /// Gets the App ID of the current process.
            /// </summary>
            public static AppId Id => SteamClient.AppId;
            /// <summary>
            /// Gets the buildid of this app, may change at any time based on backend updates to the game.
            /// </summary>
            public static int BuildId => SteamApps.BuildId;
            /// <summary>
            /// Gets the install folder for a specific AppID.
            /// </summary>
            public static string InstallDirectory => SteamApps.AppInstallDir(Id);
            /// <summary>
            /// Gets the number of DLC pieces for the current app.
            /// </summary>
            public static int DLCCount => SteamApps.DlcInformation().Count();
            /// <summary>
            /// Gets the command line if the game was launched via Steam URL, e.g. steam://run/&lt;appid&gt;//&lt;command line&gt;/. This method is preferable to launching with a command line via the operating system, which can be a security risk. In order for rich presence joins to go through this and not be placed on the OS command line, you must enable "Use launch command line" from the Installation &gt; General page on your app.
            /// </summary>
            public static string LaunchCommandLine => SteamApps.CommandLine;

            /// <summary>
            /// Checks if a specific app is installed.
            /// </summary>
            /// <remarks>
            /// The app may not actually be owned by the current user, they may have it left over from a free weekend, etc.
            /// This only works for base applications, not Downloadable Content(DLC). Use IsDlcInstalled for DLC instead.
            /// </remarks>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static bool IsAppInstalled(AppId appId) => SteamApps.IsAppInstalled(appId);
            /// <summary>
            /// Checks if the user owns a specific DLC and if the DLC is installed
            /// </summary>
            /// <param name="appId">The App ID of the DLC to check.</param>
            /// <returns></returns>
            public static bool IsDlcInstalled(AppId appId) => SteamApps.IsDlcInstalled(appId);
            /// <summary>
            /// Gets the download progress for optional DLC.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="bytesDownloaded"></param>
            /// <param name="bytesTotal"></param>
            /// <returns></returns>
            public static bool GetDlcDownloadProgress(AppId appId, out ulong bytesDownloaded, out ulong bytesTotal)
            {
                var results = SteamApps.DlcDownloadProgress(appId);
                bytesDownloaded = results.BytesDownloaded;
                bytesTotal = results.BytesTotal;
                return results.Active;
            }
            /// <summary>
            /// Gets the install directory of the app if any
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static string GetAppInstallDirectory(AppId appId) => SteamApps.AppInstallDir(appId);
            /// <summary>
            /// Returns the collection of installed depots in mount order
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static Steamworks.Data.DepotId[] InstalledDepots(AppId appId) => SteamApps.InstalledDepots(appId).ToArray();
            /// <summary>
            /// Parameter names starting with the character '@' are reserved for internal use and will always return an empty string. Parameter names starting with an underscore '_' are reserved for steam features -- they can be queried by the game, but it is advised that you not param names beginning with an underscore for your own features.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static string QueryLaunchParam(string key) => SteamApps.GetLaunchParam(key);
            /// <summary>
            /// Install an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void InstallDLC(AppId appId) => SteamApps.InstallDlc(appId);
            /// <summary>
            /// Uninstall an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void UninstallDLC(AppId appId) => SteamApps.UninstallDlc(appId);
            /// <summary>
            /// Checks if the active user is subscribed to a specified appId.
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static bool IsSubscribedApp(AppId appId) => SteamApps.IsSubscribedToApp(appId);
            [Obsolete("Facepunch does not support this feature, use Steamworks.NET if this is required")]
            public static bool IsTimedTrial(out uint secondsAllowed, out uint secondsPlayed)
            {
                secondsAllowed = 0;
                secondsPlayed = 0;
                return false;
            }
            /// <summary>
            /// Gets the current beta branch name if any
            /// </summary>
            /// <param name="name">outputs the name of the current beta branch if any</param>
            /// <returns>True if the user is running from a beta branch</returns>
            public static bool GetCurrentBetaName(out string name)
            {
                name = SteamApps.CurrentBetaName;
                return name != null;
            }
            /// <summary>
            /// Gets the time of purchase of the specified app
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static DateTime GetEarliestPurchaseTime(AppId appId) => SteamApps.PurchaseTime(appId);
            /// <summary>
            /// Asynchronously retrieves metadata details about a specific file in the depot manifest.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="callback"></param>
            public static void GetFileDetails(string name, Action<Steamworks.Data.FileDetails?> callback)
            {
                if (callback == null)
                    return;

                SteamSettings.behaviour.StartCoroutine(GetFileDetailsAsync(name, callback));
            }
            private static IEnumerator GetFileDetailsAsync(string name, Action<Steamworks.Data.FileDetails?> callback)
            {
                var task = SteamApps.GetFileDetailsAsync(name);
                yield return new WaitUntil(() => { return task.IsCompleted; });

                callback?.Invoke(task.Result);
            }

            /// <summary>
            /// If you detect the game is out-of-date (for example, by having the client detect a version mismatch with a server), you can call use MarkContentCorrupt to force a verify, show a message to the user, and then quit.
            /// </summary>
            /// <param name="missingFilesOnly"></param>
            /// <returns></returns>
            public static void MarkContentCorrupt(bool missingFilesOnly) => SteamApps.MarkContentCorrupt(missingFilesOnly);

        }
#endif
#if STEAMWORKSNET
        public static class Web
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                appsListApi = default;
                appsListLoaded = false;
                appsListLoading = false;
            }

            [Serializable]
            private struct SteamAppsListAPI
            {
                [Serializable]
                public struct Model
                {
                    [Serializable]
                    public struct AppData
                    {
                        public ulong appid;
                        public string name;
                    }

                    public AppData[] apps;
                }

                public Model applist;

                public static UnityWebRequest GetRequest()
                {
                    return UnityWebRequest.Get("https://api.steampowered.com/ISteamApps/GetAppList/v2/");
                }
            }

            [Serializable]
            public struct SteamAppNews
            {
                [Serializable]
                public struct SteamNewsItem
                {
                    public ulong gid;
                    public string title;
                    public string url;
                    public bool is_external_url;
                    public string author;
                    public string contents;
                    public string feedlabel;
                    public long date;
                    public string feedname;
                    public uint feed_type;
                    public uint appid;

                    public DateTime Date => new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(date);
                }

                public uint appid;
                public SteamNewsItem[] newsitems;
                public uint count;
            }

            private static bool appsListLoaded = false;
            private static bool appsListLoading = false;
            private static SteamAppsListAPI appsListApi;

            private static IEnumerator GetAppList(Action callback)
            {
                if (!appsListLoaded)
                {
                    if (!appsListLoading)
                    {
                        appsListLoading = true;

                        yield return new WaitForEndOfFrame();
                        var www = SteamAppsListAPI.GetRequest();

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                appsListApi = JsonUtility.FromJson<SteamAppsListAPI>(resultContent);

                                appsListLoaded = true;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("Failed to load the Steam App List: Exception = " + ex.Message);
                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to load the Steam App List: Error = " + www.error);
                        }

                        appsListLoading = false;
                    }
                    else
                    {
                        while (appsListLoading)
                            yield return null;
                    }
                }

                callback?.Invoke();
            }

            public static bool IsAppsListLoaded => appsListLoaded;

            /// <summary>
            /// Requests the list of all Steam apps from the Steam Web API.
            /// This must be called before the <see cref="GetAppName(AppId_t, out string)"/> can work.
            /// The <see cref="GetAppName(AppId_t, Action{string, bool})"/> will call this for you if requried.
            /// </summary>
            /// <param name="callback"></param>
            public static void LoadAppNames(Action callback)
            {
                SteamSettings.behaviour.StartCoroutine(GetAppList(callback));
            }

            /// <summary>
            /// This calls assumes you have already called <see cref="LoadAppNames(Action)"/> and simply returns the name of the indicated app if known.
            /// </summary>
            /// <param name="appId">The app to read the name for</param>
            /// <param name="name">The name found if any</param>
            /// <returns><see cref="true"/> if the app was found, <see cref="false"/> otherwise</returns>
            public static bool GetAppName(AppId_t appId, out string name)
            {
                if (appsListApi.applist.apps != null && appsListApi.applist.apps.Length > 0)
                {
                    var app = appsListApi.applist.apps.FirstOrDefault(p => p.appid == appId.m_AppId);
                    if (app.appid == appId.m_AppId)
                    {
                        name = app.name;
                        return true;
                    }
                    else
                    {
                        name = "Unknown";
                        return false;
                    }

                }
                else
                {
                    name = "Unknown";
                    return false;
                }
            }
            /// <summary>
            /// Gets the app name invoking the callback immeaditly if the names are already loaded. If not this will load the names and then invoke the callback when read.
            /// </summary>
            /// <remarks>
            /// Callback signature should be
            /// <code>
            /// void HandleCallback(string name, bool ioFailure);
            /// </code>
            /// The name paramiter is the name found if any and the ioFailure paramiter is true if an error occured or the app was not found.
            /// </remarks>
            /// <param name="appId">The App ID to find the name for</param>
            /// <param name="callback">The callback to invoke when found, the <see cref="string"/> will be the name found if any, the <see cref="bool"/> will be true if an error occured</param>
            public static void GetAppName(AppId_t appId, Action<string, bool> callback)
            {
                if (!appsListLoaded)
                {
                    SteamSettings.behaviour.StartCoroutine(GetAppList(() =>
                    {
                        if (GetAppName(appId, out string name))
                            callback?.Invoke(name, false);
                        else
                            callback?.Invoke("Unkown", true);
                    }));
                }
                else
                {
                    if (GetAppName(appId, out string name))
                        callback?.Invoke(name, false);
                    else
                        callback?.Invoke("Unkown", true);
                }
            }

            /// <summary>
            /// Gets the news entries for the specified app as they are seen in the Steam Community New listing
            /// </summary>
            /// <param name="appId">The app to read the news for</param>
            /// <param name="count">The number of entries to return, if left to 0 the Steam default will return (20 at the time of writing)</param>
            /// <param name="feeds">The comma delimited list of feeds to be read, leave blank or pass in <see cref="string.Empty"/> to return all feeds</param>
            /// <param name="tags">The comma delimited list of tags to be read, leave blank or pass in <see cref="string.Empty"/> to return all tags</param>
            /// <param name="callback">This will be invoked when the call is complete, if null no call will be made. The <see cref="SteamAppNews"/> paramiter is the results found, the <see cref="bool"/> paramiter is true when an error occured i.e. IOFailure</param>
            public static void GetNewsForApp(AppId_t appId, uint count, string feeds, string tags, Action<SteamAppNews, bool> callback)
            {
                if (callback != null)
                {
                    string get = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=" + appId.m_AppId.ToString();
                    if (count > 0)
                        get += "&count=" + count.ToString();
                    if (!string.IsNullOrEmpty(feeds))
                        get += "&feeds=" + feeds.ToString();
                    if (!string.IsNullOrEmpty(tags))
                        get += "&tags=" + tags.ToString();

                    SteamSettings.behaviour.StartCoroutine(GetNewsForApp(new UnityWebRequest(get), callback));
                }
            }

            private static IEnumerator GetNewsForApp(UnityWebRequest www, Action<SteamAppNews, bool> callback)
            {
                var co = www.SendWebRequest();
                while (!co.isDone)
                    yield return null;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string resultContent = www.downloadHandler.text;
                        callback?.Invoke(JsonUtility.FromJson<SteamAppNews>(resultContent), false);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to load the Steam App News: Exception = " + ex.Message);
                        callback?.Invoke(default, true);
                    }
                }
                else
                {
                    Debug.LogError("Failed to load the Steam App News: Error = " + www.error);
                    callback?.Invoke(default, true);
                }
            }
        }
#elif FACEPUNCH
        public static class Web
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                appsListApi = default;
                appsListLoaded = false;
                appsListLoading = false;
            }

            [Serializable]
            private struct SteamAppsListAPI
            {
                [Serializable]
                public struct Model
                {
                    [Serializable]
                    public struct AppData
                    {
                        public ulong appid;
                        public string name;
                    }

                    public AppData[] apps;
                }

                public Model applist;

                public static UnityWebRequest GetRequest()
                {
                    return UnityWebRequest.Get("https://api.steampowered.com/ISteamApps/GetAppList/v2/");
                }
            }

            [Serializable]
            public struct SteamAppNews
            {
                [Serializable]
                public struct SteamNewsItem
                {
                    public ulong gid;
                    public string title;
                    public string url;
                    public bool is_external_url;
                    public string author;
                    public string contents;
                    public string feedlabel;
                    public long date;
                    public string feedname;
                    public uint feed_type;
                    public uint appid;

                    public DateTime Date => new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(date);
                }

                public uint appid;
                public SteamNewsItem[] newsitems;
                public uint count;
            }

            private static bool appsListLoaded = false;
            private static bool appsListLoading = false;
            private static SteamAppsListAPI appsListApi;

            private static IEnumerator GetAppList(Action callback)
            {
                if (!appsListLoaded)
                {
                    if (!appsListLoading)
                    {
                        appsListLoading = true;

                        yield return new WaitForEndOfFrame();
                        var www = SteamAppsListAPI.GetRequest();

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                appsListApi = JsonUtility.FromJson<SteamAppsListAPI>(resultContent);

                                appsListLoaded = true;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("Failed to load the Steam App List: Exception = " + ex.Message);
                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to load the Steam App List: Error = " + www.error);
                        }

                        appsListLoading = false;
                    }
                    else
                    {
                        while (appsListLoading)
                            yield return null;
                    }
                }

                callback?.Invoke();
            }

            public static bool IsAppsListLoaded => appsListLoaded;

            /// <summary>
            /// Requests the list of all Steam apps from the Steam Web API.
            /// This must be called before the <see cref="GetAppName(AppId_t, out string)"/> can work.
            /// The <see cref="GetAppName(AppId_t, Action{string, bool})"/> will call this for you if requried.
            /// </summary>
            /// <param name="callback"></param>
            public static void LoadAppNames(Action callback)
            {
                SteamSettings.behaviour.StartCoroutine(GetAppList(callback));
            }

            /// <summary>
            /// This calls assumes you have already called <see cref="LoadAppNames(Action)"/> and simply returns the name of the indicated app if known.
            /// </summary>
            /// <param name="appId">The app to read the name for</param>
            /// <param name="name">The name found if any</param>
            /// <returns><see cref="true"/> if the app was found, <see cref="false"/> otherwise</returns>
            public static bool GetAppName(AppId appId, out string name)
            {
                if (appsListApi.applist.apps != null && appsListApi.applist.apps.Length > 0)
                {
                    var app = appsListApi.applist.apps.FirstOrDefault(p => p.appid == appId.Value);
                    if (app.appid == appId.Value)
                    {
                        name = app.name;
                        return true;
                    }
                    else
                    {
                        name = "Unknown";
                        return false;
                    }

                }
                else
                {
                    name = "Unknown";
                    return false;
                }
            }
            /// <summary>
            /// Gets the app name invoking the callback immeaditly if the names are already loaded. If not this will load the names and then invoke the callback when read.
            /// </summary>
            /// <remarks>
            /// Callback signature should be
            /// <code>
            /// void HandleCallback(string name, bool ioFailure);
            /// </code>
            /// The name paramiter is the name found if any and the ioFailure paramiter is true if an error occured or the app was not found.
            /// </remarks>
            /// <param name="appId">The App ID to find the name for</param>
            /// <param name="callback">The callback to invoke when found, the <see cref="string"/> will be the name found if any, the <see cref="bool"/> will be true if an error occured</param>
            public static void GetAppName(AppId appId, Action<string, bool> callback)
            {
                if (!appsListLoaded)
                {
                    SteamSettings.behaviour.StartCoroutine(GetAppList(() =>
                    {
                        if (GetAppName(appId, out string name))
                            callback?.Invoke(name, false);
                        else
                            callback?.Invoke("Unkown", true);
                    }));
                }
                else
                {
                    if (GetAppName(appId, out string name))
                        callback?.Invoke(name, false);
                    else
                        callback?.Invoke("Unkown", true);
                }
            }

            /// <summary>
            /// Gets the news entries for the specified app as they are seen in the Steam Community New listing
            /// </summary>
            /// <param name="appId">The app to read the news for</param>
            /// <param name="count">The number of entries to return, if left to 0 the Steam default will return (20 at the time of writing)</param>
            /// <param name="feeds">The comma delimited list of feeds to be read, leave blank or pass in <see cref="string.Empty"/> to return all feeds</param>
            /// <param name="tags">The comma delimited list of tags to be read, leave blank or pass in <see cref="string.Empty"/> to return all tags</param>
            /// <param name="callback">This will be invoked when the call is complete, if null no call will be made. The <see cref="SteamAppNews"/> paramiter is the results found, the <see cref="bool"/> paramiter is true when an error occured i.e. IOFailure</param>
            public static void GetNewsForApp(AppId appId, uint count, string feeds, string tags, Action<SteamAppNews, bool> callback)
            {
                if (callback != null)
                {
                    string get = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=" + appId.Value.ToString();
                    if (count > 0)
                        get += "&count=" + count.ToString();
                    if (!string.IsNullOrEmpty(feeds))
                        get += "&feeds=" + feeds.ToString();
                    if (!string.IsNullOrEmpty(tags))
                        get += "&tags=" + tags.ToString();

                    SteamSettings.behaviour.StartCoroutine(GetNewsForApp(new UnityWebRequest(get), callback));
                }
            }

            private static IEnumerator GetNewsForApp(UnityWebRequest www, Action<SteamAppNews, bool> callback)
            {
                var co = www.SendWebRequest();
                while (!co.isDone)
                    yield return null;

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string resultContent = www.downloadHandler.text;
                        callback?.Invoke(JsonUtility.FromJson<SteamAppNews>(resultContent), false);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to load the Steam App News: Exception = " + ex.Message);
                        callback?.Invoke(default, true);
                    }
                }
                else
                {
                    Debug.LogError("Failed to load the Steam App News: Error = " + www.error);
                    callback?.Invoke(default, true);
                }
            }
        }
#endif
    }
}
#endif