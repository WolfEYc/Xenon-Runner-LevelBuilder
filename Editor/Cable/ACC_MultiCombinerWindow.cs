namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class ACC_MultiCombinerWindow : EditorWindow
    {
        public static ACC_MultiCombinerWindow combinerWindow;
        private SerializedObject so;

        public ACC_Trail[] cableTrails = new ACC_Trail[0];

        public bool generateLightmapUVs = true;
        public bool doubleSided = false;
        public float backsideOffset = 0.005f;
        public string savePath = "Assets/NOT_Lonely/Advanced Cable Creator/CombinedMeshes";

        private void OnEnable()
        {
            ScriptableObject target = this;
            so = new SerializedObject(target);
        }

        [MenuItem("Tools/NOT_Lonely/Advanced Cable Creator/Cable Combiner", false, 12)]
        public static void OpenWindow()
        {
            combinerWindow = GetWindow<ACC_MultiCombinerWindow>();
            combinerWindow.titleContent = new GUIContent("Cable Combiner");

            combinerWindow.maxSize = new Vector2(1000, 1000);
            combinerWindow.minSize = new Vector2(400, 330);
        }

        private void OnGUI()
        {
            so.Update();
            SerializedProperty _cableTrails = so.FindProperty("cableTrails");
            SerializedProperty _generateLightmapUVs = so.FindProperty("generateLightmapUVs");
            SerializedProperty _doubleSided = so.FindProperty("doubleSided");
            SerializedProperty _backsideOffset = so.FindProperty("backsideOffset");
            SerializedProperty _savePath = so.FindProperty("savePath");

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            GUILayout.Label("1. Drop 'Cable Trail' objects into the list below.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5);
            GUILayout.EndVertical();

            EditorGUILayout.PropertyField(_cableTrails, new GUIContent("Cable Trails"), true);
            GUILayout.Space(15);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            GUILayout.Label("2. Adjust the combining settings.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5);
            GUILayout.EndVertical();


            EditorGUILayout.PropertyField(_generateLightmapUVs, new GUIContent("Generate Lightmap UVs", "If checked then a second UV channel will be generated for the lightmapping."));
            EditorGUILayout.PropertyField(_doubleSided, new GUIContent("Backside", "Enable this ckeckbox to generate a backside of the cable mesh. \nUseful for the flat meshes which consist of 2 radial segments."));
            if (doubleSided)
            {
                EditorGUILayout.PropertyField(_backsideOffset, new GUIContent("Offset", "How far the backside should be moved away from the frontside along the normals."));
                backsideOffset = Mathf.Max(0, backsideOffset);
            }
            EditorGUILayout.PropertyField(_savePath, new GUIContent("Save path", "Combined meshes will be saved into this folder."));
            GUILayout.Space(15);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            GUILayout.Label("3. Press the 'Combine' button.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5);
            GUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(cableTrails.Length == 0);
            if(GUILayout.Button(new GUIContent("Combine", "Combine Cable Trails into a single mesh.")))
            {
                Combine();
            }
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
            }
        }

        private void Combine()
        {
            bool allowCombining = false;

            List<Transform> initParents = new List<Transform>();

            if (cableTrails.Length > 0)
            {
                bool option = EditorUtility.DisplayDialog("Combine Cable Meshes", "You are about to combine all selected cable meshes into a single mesh. \n\n This will improve performance and allow you to bake a static lighting on them. \n\n The original cables will be hidden and you will be able to use them again if you need. \n\n Do you want to continue?", "Yes", "No");

                switch (option)
                {
                    case true:
                        allowCombining = true;
                        break;

                    case false:
                        break;
                }

                if (!allowCombining)
                {
                    return;
                }

                List<ACC_Cable> cables = new List<ACC_Cable>();
                List<ACC_CableJoint> cableJoints = new List<ACC_CableJoint>();
                List<int> siblingIDs = new List<int>();

                GameObject tempParent = new GameObject("Cables");
                tempParent.transform.SetPositionAndRotation(cableTrails[0].transform.position, cableTrails[0].transform.rotation);

                int maxSiblingID = 0;
                for (int i = 0; i < cableTrails.Length; i++)
                {
                    int curSiblingID = cableTrails[i].transform.GetSiblingIndex();
                    if (maxSiblingID < curSiblingID) maxSiblingID = curSiblingID;
                }
                tempParent.transform.SetSiblingIndex(maxSiblingID + 1);

                for (int i = 0; i < cableTrails.Length; i++)
                {
                    siblingIDs.Add(cableTrails[i].transform.GetSiblingIndex());
                    ACC_Cable[] _cables = cableTrails[i].GetComponentsInChildren<ACC_Cable>();

                    for (int j = 0; j < _cables.Length; j++)
                    {
                        cables.Add(_cables[j]);
                    }

                    ACC_CableJoint[] _cableJoints = cableTrails[i].GetComponentsInChildren<ACC_CableJoint>();

                    for (int j = 0; j < _cableJoints.Length; j++)
                    {
                        cableJoints.Add(_cableJoints[j]);
                    }

                    initParents.Add(cableTrails[i].transform.parent);
                    cableTrails[i].transform.parent = tempParent.transform;
                }

                ACC_combiner.Combine(cables.ToArray(), cableJoints.ToArray(), tempParent.transform, savePath, doubleSided, backsideOffset, generateLightmapUVs);

                //restore parents
                for (int i = 0; i < cableTrails.Length; i++)
                {
                    cableTrails[i].transform.parent = initParents[i];
                    cableTrails[i].transform.SetSiblingIndex(siblingIDs[i]);
                    cableTrails[i].gameObject.SetActive(false);
                }

                DestroyImmediate(tempParent);

                cables.Clear();
                cableJoints.Clear();
                siblingIDs.Clear();
            }
            else
            {
                bool popup = EditorUtility.DisplayDialog("Nothing selected!", "Please, select atleast one Cable Trail object in the scene to combine.", "Ok");

                switch (popup)
                {
                    case true:
                        break;

                    case false:
                        break;
                }
            }
        }
    }
}
