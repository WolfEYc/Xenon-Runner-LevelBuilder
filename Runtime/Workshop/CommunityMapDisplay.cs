using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunSim
{
    public class CommunityMapDisplay : MonoBehaviour, IDamageable
    {
        AssetBundleUGC _assetBundleUgc;

        [SerializeField] RawImage displayImage;
        [SerializeField] TMP_Text mapName;
        [SerializeField] TMP_Text status;
        
        public void AssignAssetBundle(AssetBundleUGC assetBundleUgc)
        {
            gameObject.SetActive(true);
            _assetBundleUgc = assetBundleUgc;
            
            _assetBundleUgc.CommunityItem.previewImageUpdated.AddListener(UpdateImage);
            UpdateImage();
            mapName.SetText(_assetBundleUgc.CommunityItem.Title);
            StartCoroutine(CheckUpdate());
        }

        void UpdateImage()
        {
            displayImage.texture = _assetBundleUgc.CommunityItem.previewImage;
        }

        IEnumerator CheckUpdate()
        {
            while (true)
            {
                status.SetText(_assetBundleUgc.GetStatus());
                yield return ExtensionMethods.SlowPollUpdate;
            }
        }

        public void Damage()
        {
            _assetBundleUgc?.LoadScene();
        }
    }
}
