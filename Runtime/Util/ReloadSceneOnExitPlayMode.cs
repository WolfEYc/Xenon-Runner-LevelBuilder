#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


namespace SpeedrunSim
{
    [InitializeOnLoad]
    public static class ReloadSceneOnExitPlayMode
    {
        static ReloadSceneOnExitPlayMode()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.EnteredEditMode) return;

            EditorSceneManager.OpenScene("Assets/_Root/Scenes/StartRoom.unity");
        }
    }


}

#endif

