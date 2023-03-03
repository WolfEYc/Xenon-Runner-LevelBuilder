
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;

namespace SpeedrunSim
{
    [CreateAssetMenu(menuName = "Workshop Level", fileName = "NewWorkshopLevel", order = 5)]
    public class SceneBundleUpload : ScriptableObject
    {
        [SerializeField] WorkshopItemData itemData;

        public string bundleFilePath;
        public PublishedFileId_t publishedExistingFile;
        static bool _updating;

        void Awake()
        {
            _updating = false;
        }

        void RenameLevelFile()
        {
            File.Move(bundleFilePath, Path.Join(itemData.contentFolder, "level"));
        }

        
        public void Upload()
        {
            if(publishedExistingFile.m_PublishedFileId != 0 || _updating)
            {
                Debug.LogWarning("Already Created or Updating");
                return;
            }

            _updating = true;

            bundleFilePath = Path.GetFullPath(bundleFilePath);
            itemData.contentFolder = Path.GetFullPath(itemData.contentFolder);
            itemData.previewImageFile = Path.GetFullPath(itemData.previewImageFile);
            
            RenameLevelFile();
            
            
            UserGeneratedContent.Client.CreateItem(itemData, CreationCallback);
        }

        bool UpdateBoiler(out UGCUpdateHandle_t updateHandle)
        {
            updateHandle = UGCUpdateHandle_t.Invalid;
            if (publishedExistingFile.m_PublishedFileId == 0 || _updating) return false;
            updateHandle = UserGeneratedContent.Client.StartItemUpdate(itemData.appId, publishedExistingFile);
            if(updateHandle == UGCUpdateHandle_t.Invalid) return false;
            
            _updating = true;
            return true;
        }
        

        
        public void UpdateTitle()
        {
            if(!UpdateBoiler(out UGCUpdateHandle_t updateHandle)) return;
            
            UserGeneratedContent.Client.SetItemTitle(updateHandle, itemData.title);
                
            UserGeneratedContent.Client.SubmitItemUpdate(updateHandle, "updated title", ItemUpdated);
        }
        
        
        public void UpdateDescription()
        {
            if(!UpdateBoiler(out UGCUpdateHandle_t updateHandle)) return;
            
            UserGeneratedContent.Client.SetItemDescription(updateHandle, itemData.description);
                
            UserGeneratedContent.Client.SubmitItemUpdate(updateHandle, "updated description", ItemUpdated);
        }
        
        
        public void UpdateContentFolder()
        {
            if(!UpdateBoiler(out UGCUpdateHandle_t updateHandle)) return;
            
            RenameLevelFile();

            UserGeneratedContent.Client.SetItemContent(updateHandle, itemData.contentFolder);
                
            UserGeneratedContent.Client.SubmitItemUpdate(updateHandle, "updated content", ItemUpdated);
        }
        
        
        public void UpdatedPreviewImageFile()
        {
            if(!UpdateBoiler(out UGCUpdateHandle_t updateHandle)) return;
            
            UserGeneratedContent.Client.SetItemPreview(updateHandle, itemData.contentFolder);
                
            UserGeneratedContent.Client.SubmitItemUpdate(updateHandle, "updated preview image", ItemUpdated);
        }
        
        
        public void UpdatedMetaData()
        {
            if(!UpdateBoiler(out UGCUpdateHandle_t updateHandle)) return;
            
            UserGeneratedContent.Client.SetItemMetadata(updateHandle, itemData.contentFolder);
                
            UserGeneratedContent.Client.SubmitItemUpdate(updateHandle, "updated metadata", ItemUpdated);
        }
        
        
        public void UpdateTags()
        {
            if(!UpdateBoiler(out UGCUpdateHandle_t updateHandle)) return;
            
            UserGeneratedContent.Client.SetItemTags(updateHandle, new List<string>(itemData.tags));
                
            UserGeneratedContent.Client.SubmitItemUpdate(updateHandle, "updated tags", ItemUpdated);
        }
        
        
        public void UpdateVisibility()
        {
            if(!UpdateBoiler(out UGCUpdateHandle_t updateHandle)) return;
            
            UserGeneratedContent.Client.SetItemVisibility(updateHandle, itemData.visibility);
                
            UserGeneratedContent.Client.SubmitItemUpdate(updateHandle, "updated visibility", ItemUpdated);
        }

        
        void ItemUpdated(SubmitItemUpdateResult_t result, bool failure)
        {
            _updating = false;
            Debug.Log(result.m_eResult.ToString());
        }

        void CreationCallback(WorkshopItemDataCreateStatus obj)
        {
            _updating = false;
            if (obj.hasError)
            {
                if (obj.ugcFileId.HasValue)
                {
                    publishedExistingFile = obj.ugcFileId.Value;
                    UserGeneratedContent.Client.DeleteItem(obj.ugcFileId.Value, (deleteResult, failure) =>
                    {
                        if (deleteResult.m_eResult == EResult.k_EResultOK)
                        {
                            publishedExistingFile.m_PublishedFileId = 0;
                        }
                    });
                }

                Debug.LogError(obj.errorMessage, this);
            }
            else
            {
                if (obj.ugcFileId.HasValue)
                {
                    publishedExistingFile = obj.ugcFileId.Value;
                }
                
                Debug.Log(obj.createItemResult.ToString(), this);
            }
        }
    }
}

#endif
