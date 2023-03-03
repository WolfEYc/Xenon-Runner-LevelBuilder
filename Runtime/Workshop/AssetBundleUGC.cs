using System.IO;
using HeathenEngineering.SteamworksIntegration;
using UnityEngine;

namespace SpeedrunSim
{
    public class AssetBundleUGC
    {
        SingleSceneAssetBundle _singleSceneAssetBundle;
        public readonly UGCCommunityItem CommunityItem;
        AssetBundleCreateRequest _req;
        
        public bool SceneReadyToLoad => _singleSceneAssetBundle != null;
        public bool Installed => CommunityItem.IsInstalled && !CommunityItem.IsNeedsUpdate;

        string AssetFilePath => Path.Join(CommunityItem.FolderPath, "level");

        bool _loading;
        

        public AssetBundleUGC(UGCCommunityItem communityItem)
        {
            CommunityItem = communityItem;
            
            Debug.Log($"Listing {CommunityItem.Title} level in filepath {AssetFilePath}");
        }

        void Download()
        {
            if(Installed) return;
            CommunityItem.DownloadItem(true);
        }
        
        public void LoadScene()
        {
            if (!Installed)
            {
                Download();
                return;
            }
            
            if (!SceneReadyToLoad)
            {
                LoadFromFile();
                return;
            }
            
            _singleSceneAssetBundle.LoadScene();
        }

        void LoadFromFile()
        {
            if(_loading) return;
            Debug.Log(AssetFilePath);
            
            
            _req = AssetBundle.LoadFromFileAsync(AssetFilePath);
            _req.completed += FileLoadComplete;
            _loading = true;
        }

        void FileLoadComplete(AsyncOperation obj)
        {
            _singleSceneAssetBundle = new SingleSceneAssetBundle(_req.assetBundle);
            _loading = false;
        }
        
        public string GetStatus()
        {
            return Installed switch
            {
                true when SceneReadyToLoad => _singleSceneAssetBundle.Status(),
                true when _loading => $"Preparing {_req.progress.ToPercent()}%",
                true => $"On Disk",
                false when CommunityItem.IsDownloadPending => "Download Pending",
                false when CommunityItem.IsDownloading => $"Downloading {CommunityItem.DownloadCompletion.ToPercent()}%",
                false => $"Shoot to Download"
            };
        }
    }
}
