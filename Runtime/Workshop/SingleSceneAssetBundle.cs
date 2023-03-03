using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunSim
{
    public class SingleSceneAssetBundle
    {
        readonly string _scenePath;
        AsyncOperation _loadOperation;
        static bool _loading;

        public bool IsCurrentScene => SceneManager.GetActiveScene().path == _scenePath;
        public bool IsLoadingScene { get; private set; }
        public float LoadingProgress => _loadOperation.progress;
        
        public SingleSceneAssetBundle(AssetBundle assetBundle)
        {
            _scenePath = assetBundle.GetAllScenePaths()[0];
        }

        public void LoadScene()
        {
            if(_loading || IsCurrentScene) return;
            _loading = true;
            IsLoadingScene = true;
            _loadOperation = SceneManager.LoadSceneAsync(_scenePath);
            _loadOperation.completed += OnCompleted;
        }

        void OnCompleted(AsyncOperation obj)
        {
            _loading = false;
            IsLoadingScene = false;
        }

        public string Status()
        {
            return IsCurrentScene switch
            {
                true => "Current Map",
                false when IsLoadingScene => $"Loading {LoadingProgress.ToPercent()}%",
                false => $"Prepared"
            };
        }
    }
    
}
