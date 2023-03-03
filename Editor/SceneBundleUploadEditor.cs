using SpeedrunSim;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneBundleUpload))]
public class SceneBundleUploadEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var sceneBundleUpload = (SceneBundleUpload)target;
 
        if(GUILayout.Button("Upload", GUILayout.Height(30)))
        {
            sceneBundleUpload.Upload();
            return;
        }
        
        if(GUILayout.Button("UpdateTitle", GUILayout.Height(30)))
        {
            sceneBundleUpload.UpdateTitle();
            return;
        }
         
        if(GUILayout.Button("UpdateDescription", GUILayout.Height(30)))
        {
            sceneBundleUpload.UpdateDescription();
            return;
        }
        
        if(GUILayout.Button("UpdateContentFolder", GUILayout.Height(30)))
        {
            sceneBundleUpload.UpdateContentFolder();
            return;
        }
        
        if(GUILayout.Button("UpdatedPreviewImageFile", GUILayout.Height(30)))
        {
            sceneBundleUpload.UpdatedPreviewImageFile();
            return;
        }
        
        if(GUILayout.Button("UpdatedMetaData", GUILayout.Height(30)))
        {
            sceneBundleUpload.UpdatedMetaData();
            return;
        }
        
        if(GUILayout.Button("UpdateTags", GUILayout.Height(30)))
        {
            sceneBundleUpload.UpdateTags();
            return;
        }
        
        if(GUILayout.Button("UpdateVisibility", GUILayout.Height(30)))
        {
            sceneBundleUpload.UpdateVisibility();
            return;
        }
    }
}
