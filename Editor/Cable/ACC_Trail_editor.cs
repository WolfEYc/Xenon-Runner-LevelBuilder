#if UNITY_EDITOR
namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ACC_Trail))]
    public class ACC_Trail_editor : Editor
    {
        ACC_Trail cablesTrail;

        SerializedProperty controlPoints;
        SerializedProperty pointsOffset;

        SerializedProperty stepSize;
        SerializedProperty positionRandomness;

        SerializedProperty radialSegments;
        SerializedProperty angle;
        SerializedProperty lengthSegments;
        SerializedProperty invert;
        SerializedProperty vertexAlphaBrightness;
        SerializedProperty textureTilingMultiplier;
        SerializedProperty textureOffsetRandomness;
        SerializedProperty uvsAngle;
        SerializedProperty uvTwist;
        SerializedProperty materials;

        SerializedProperty heightScale;
        SerializedProperty lengthDependentHeight;
        SerializedProperty thickness;
        SerializedProperty horizontalCurvature;
        SerializedProperty verticalCurvature;
        SerializedProperty noise;

        SerializedProperty jointObj;
        SerializedProperty jointRotation;
        SerializedProperty jointObjScale;
        SerializedProperty uniformScale;
        SerializedProperty scale;

        SerializedProperty generateLightmapUVs;
        SerializedProperty generateBackside;
        SerializedProperty backsideDistance;
        SerializedProperty savePath;

        private bool controlPointsProperties = true;
        private bool trailProperties = true;
        private bool meshProperties = true;
        private bool shapeProperties = true;
        private bool jointsProperties = true;
        private bool editorPerformanceProperties = true;
        private bool optimizationProperties = true;

        private bool combinePressed = false;
        private bool amountPressed = false;

        SerializedProperty showMeshes;

        private Texture2D icon;

        private void OnEnable()
        {
            controlPoints = serializedObject.FindProperty("controlPoints");
            pointsOffset = serializedObject.FindProperty("pointsOffset");

            stepSize = serializedObject.FindProperty("stepSize");
            positionRandomness = serializedObject.FindProperty("positionRandomness");

            radialSegments = serializedObject.FindProperty("radialSegments");
            angle = serializedObject.FindProperty("angle");
            lengthSegments = serializedObject.FindProperty("lengthSegments");
            invert = serializedObject.FindProperty("invert");
            vertexAlphaBrightness = serializedObject.FindProperty("vertexAlphaBrightness");
            textureTilingMultiplier = serializedObject.FindProperty("textureTilingMultiplier");
            textureOffsetRandomness = serializedObject.FindProperty("textureOffsetRandomness");
            uvsAngle = serializedObject.FindProperty("uvsAngle");
            uvTwist = serializedObject.FindProperty("uvTwist");
            materials = serializedObject.FindProperty("materials");

            heightScale = serializedObject.FindProperty("heightScale");
            lengthDependentHeight = serializedObject.FindProperty("lengthDependentHeight");
            thickness = serializedObject.FindProperty("thickness");
            horizontalCurvature = serializedObject.FindProperty("horizontalCurvature");
            verticalCurvature = serializedObject.FindProperty("verticalCurvature");
            noise = serializedObject.FindProperty("noise");

            jointObj = serializedObject.FindProperty("jointObj");
            jointRotation = serializedObject.FindProperty("jointRotation");
            jointObjScale = serializedObject.FindProperty("jointObjScale");
            uniformScale = serializedObject.FindProperty("uniformScale");
            scale = serializedObject.FindProperty("scale");

            generateLightmapUVs = serializedObject.FindProperty("generateLightmapUVs");
            generateBackside = serializedObject.FindProperty("generateBackside");
            backsideDistance = serializedObject.FindProperty("backsideDistance");
            savePath = serializedObject.FindProperty("savePath");

            showMeshes = serializedObject.FindProperty("showMeshes");

            Undo.undoRedoPerformed += OnUndoRedoPerformed;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += SceneViewHandles;
#else
            SceneView.onSceneGUIDelegate += SceneViewHandles;
#endif
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += SceneViewHandles;
#else
            SceneView.onSceneGUIDelegate += SceneViewHandles;
#endif
        }

        public void OnUndoRedoPerformed()
        {
            if (cablesTrail == null) return;
            if (Selection.activeGameObject != cablesTrail.gameObject) return;

            cablesTrail.SetControlPoints();

            cablesTrail.SwitchShowMeshes();
            cablesTrail.UpdateSequencesPositions();

            cablesTrail.UpdateUVs();
            cablesTrail.UpdateHeightScale();
            cablesTrail.UpdateAllSequences();
        }


        [MenuItem("Tools/NOT_Lonely/Advanced Cable Creator/Create Cable Trail", false, 1)]
        [MenuItem("GameObject/NOT_Lonely/Cable Trail", false, 10)]
        public static void CreateNewCablesTrail()
        {
            ACC_Trail _cablesTrail = new GameObject("Cable Trail", typeof(ACC_Trail)).GetComponent<ACC_Trail>();

            SceneView viewport = SceneView.lastActiveSceneView;

            _cablesTrail.transform.position = viewport.camera.transform.position + (viewport.camera.transform.forward);
            Selection.activeObject = _cablesTrail;

            _cablesTrail.CreateFirstCableSegment(_cablesTrail.transform);
            _cablesTrail.UpdateAllSequences();
        }

        public override void OnInspectorGUI()
        {
            cablesTrail = target as ACC_Trail;

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);

            icon = (Texture2D)AssetDatabase.LoadAssetAtPath(cablesTrail.toolRootFolder + "/UI/Icon_trail.png", typeof(Texture2D));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(icon);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //CONTROL POINTS
            controlPointsProperties = EditorGUILayout.Foldout(controlPointsProperties, "CONTROL POINTS", true);
            if (controlPointsProperties)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(pointsOffset, new GUIContent("Offset", "Offsets the cable trail joints from the pivots of the control points."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.PropertyField(controlPoints, new GUIContent("External Control Points", "An array of scene transforms to use as control points. \nDrop at least two scene transforms here. \nElements can be reordered."), true);

                if (GUILayout.Button(new GUIContent("Update", "Press this button if you added, removed or reordered the control points and want to force update the trail.")))
                {
                    cablesTrail.SetControlPoints();
                    cablesTrail.UpdateSequencesPositions();
                    EditorUtility.SetDirty(cablesTrail);
                }

                GUILayout.EndVertical();
                GUILayout.Space(15);
            }

            //TRAIL PROPERTIES
            trailProperties = EditorGUILayout.Foldout(trailProperties, "TRAIL", true);
            if (trailProperties)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Amount", "How many cable/rope sequences to create in every trail segment?"));

                EditorGUI.BeginDisabledGroup(cablesTrail.amount == 1);
                if (GUILayout.Button(new GUIContent("<", "Decrease the amout"), GUILayout.MaxWidth(20)))
                {
                    amountPressed = true;
                    cablesTrail.ChangeAmount(-1);
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Label(cablesTrail.amount.ToString(), GUILayout.MaxWidth(cablesTrail.amount < 10 ? 12 : 18));

                EditorGUI.BeginDisabledGroup(cablesTrail.amount == 20);
                if (GUILayout.Button(new GUIContent(">", "Increase the amout"), GUILayout.MaxWidth(20)))
                {
                    amountPressed = true;
                    cablesTrail.ChangeAmount(1);
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(stepSize, new GUIContent(Vector3.Magnitude(cablesTrail.positionRandomness) > 0 ? "Step Size (max)" : "Step Size", "Distances between cable/rope sequences in their local X, Y and Z axes. \nWhen randomness used, these values are maximum step distance in the random range."));

                EditorGUILayout.PropertyField(positionRandomness, new GUIContent("Position Randomness", "Position randomness by X, Y and Z axis."));
                cablesTrail.positionRandomness = new Vector3(Mathf.Clamp01(cablesTrail.positionRandomness.x), Mathf.Clamp01(cablesTrail.positionRandomness.y), Mathf.Clamp01(cablesTrail.positionRandomness.z));
                GUILayout.EndVertical();

                GUILayout.Space(15);
            }

            //MESH
            meshProperties = EditorGUILayout.Foldout(meshProperties, "MESH", true);
            if (meshProperties)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(radialSegments, new GUIContent("Radial Segments", "Cross section segments of each cable/rope mesh."));
                cablesTrail.radialSegments = Mathf.Max(2, cablesTrail.radialSegments);

                EditorGUILayout.PropertyField(angle, new GUIContent("Angle", "A normalized angle of the cross section. \n Useful to rotate the mesh cross section when using only 2 'Radial Segments'."));
                cablesTrail.angle = Mathf.Clamp(cablesTrail.angle, 0, 1);

                EditorGUILayout.PropertyField(lengthSegments, new GUIContent("Length Segments", "Lengthwise segments count of each half of cable/rope mesh (total lengthwise segments = Length Segments * 2)"));
                cablesTrail.lengthSegments = Mathf.Max(4, cablesTrail.lengthSegments);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(invert, new GUIContent("Invert", "Inverts the mesh so you will see it from inside."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(vertexAlphaBrightness, new GUIContent("Vertex Alpha Brightness", "The brightness of the vertex a-channel, can be used for the vertex animation on a shader side."));
                GUILayout.EndVertical();
                cablesTrail.vertexAlphaBrightness = Mathf.Clamp01(cablesTrail.vertexAlphaBrightness);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(textureTilingMultiplier, new GUIContent("Texture Tiling Multiplier", "Stretch or squeeze mesh UVs: \n1 = original texture proportions, \n<1 = stretched texture, \n>1 = squeezed texture."));
                cablesTrail.textureTilingMultiplier = Mathf.Max(0.001f, cablesTrail.textureTilingMultiplier);

                EditorGUILayout.PropertyField(textureOffsetRandomness, new GUIContent("Texture Offset Randomness", "How much to randomize the texture offsets between each sequence?"));
                cablesTrail.textureOffsetRandomness = Mathf.Clamp01(cablesTrail.textureOffsetRandomness);

                EditorGUILayout.PropertyField(uvsAngle, new GUIContent("UVs angle", "The angle of the generated UVs. Use this to rotate the texture. \nOnly affects cables which use a square texture in their materials."));
                cablesTrail.uvsAngle = Mathf.Clamp(cablesTrail.uvsAngle, 0, 360);

                EditorGUILayout.PropertyField(uvTwist, new GUIContent("Texture Twist min/max", "Useful to create a twisted cables look. \nUse different values for the 'X'(min) and 'Y'(max) to randomize."));

                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(materials, new GUIContent("Materials", "A random material from this array will be applied to each Cable Sequence. \nIf this slot is empty, then a default material with a default shader will be set."), true);
                GUILayout.EndVertical();

                GUILayout.Space(15);
            }

            //SHAPE  
            shapeProperties = EditorGUILayout.Foldout(shapeProperties, "SHAPE", true);
            if (shapeProperties)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(heightScale, new GUIContent("Height Scale min/max", "The cable sagging. \nUse different values for the 'X'(min) and 'Y'(max) to randomize."));

                EditorGUILayout.PropertyField(lengthDependentHeight, new GUIContent("Length Dependency", "Values above 0 will add a dependency of the cable section height scale to the length of the cable."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(thickness, new GUIContent("Thickness min/max", "The thickness of the cable/rope mesh. \nUse different values for the 'X'(min) and 'Y'(max) to randomize."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(horizontalCurvature, new GUIContent("Horizontal Curvature", "The amount of curvature on the joints of the cable in the horizontal plane."));
                EditorGUILayout.PropertyField(verticalCurvature, new GUIContent("Vertical Curvature", "The amount of curvature on the joints of the cable in the vertical plane."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(noise, new GUIContent("Noise", "Amount of position randomness applied to the cable vertices."));
                GUILayout.EndVertical();

                cablesTrail.lengthDependentHeight = Mathf.Max(0, cablesTrail.lengthDependentHeight);
                cablesTrail.thickness.x = Mathf.Max(0.001f, cablesTrail.thickness.x);
                cablesTrail.thickness.y = Mathf.Max(0.001f, cablesTrail.thickness.y);
                cablesTrail.horizontalCurvature = Mathf.Max(0, cablesTrail.horizontalCurvature);
                cablesTrail.verticalCurvature = Mathf.Max(0, cablesTrail.verticalCurvature);
                cablesTrail.noise = Mathf.Max(0, cablesTrail.noise);

                GUILayout.Space(15);
            }

            //JOINTS
            jointsProperties = EditorGUILayout.Foldout(jointsProperties, "JOINTS", true);
            if (jointsProperties)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(jointObj, new GUIContent("Joint Object", "This object will be placed on the cable joints. Put a prefab from the Project folder here."));
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Update Joint Object", "Press this button to update the joint object, if you added a new one into the slot."), GUILayout.MaxWidth(128)))
                {
                    cablesTrail.UpdateJointObjects();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(jointRotation, new GUIContent("Rotation", "Local rotation of the joints. \nAdjust it if your joint objects oriented wrong."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(uniformScale, new GUIContent("Use Uniform Scale", "If checked, then joint objects will be scaled uniformly by all axis."));

                if (cablesTrail.uniformScale)
                {
                    EditorGUILayout.PropertyField(scale, new GUIContent("Scale", "The scale multiplier for the joint objects, relative to the cable thickness."));
                    cablesTrail.jointObjScale = Vector3.one * cablesTrail.scale;
                }
                else
                {
                    EditorGUILayout.PropertyField(jointObjScale, new GUIContent("Scale", "The scale multiplier for the joint objects, relative to the cable thickness."));
                }
                GUILayout.EndVertical();

                GUILayout.Space(15);
            }

            //OPTIMIZATION
            optimizationProperties = EditorGUILayout.Foldout(optimizationProperties, "OPTIMIZATION", true);
            if (optimizationProperties)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.PropertyField(generateLightmapUVs, new GUIContent("Generate Lightmap UVs", "If checked then a second UV channel will be generated for the lightmapping."));

                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.BeginDisabledGroup(cablesTrail.radialSegments != 2);
                EditorGUILayout.PropertyField(generateBackside, new GUIContent("Backside", "If the 'Radial Segments' set to 2, you can enable this ckeckbox to generate a backside of the cable mesh."));

                if (cablesTrail.radialSegments != 2) cablesTrail.generateBackside = false;

                if (cablesTrail.generateBackside)
                {
                    EditorGUILayout.PropertyField(backsideDistance, new GUIContent("Offset", "How far the backside should be moved away from the frontside along the normals."));
                    cablesTrail.backsideDistance = Mathf.Max(0, cablesTrail.backsideDistance);
                }

                EditorGUI.EndDisabledGroup();

                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(savePath, new GUIContent("Save Path", "Combined meshes will be saved into this folder."));
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("Combine Meshes", "Combine all cable meshes into a single mesh."), GUILayout.MaxWidth(180)))
                {
                    bool option = EditorUtility.DisplayDialog("Combine Cable Meshes", "You are about to combine all cable meshes into a single mesh. \n\n This will improve performance and allow you to bake a static lighting on them. \n\n The original cables will be hidden and you will be able to use them again if you need. \n\n Do you want to continue?", "Yes", "No");

                    switch (option)
                    {
                        case true:
                            ACC_combiner.Combine(cablesTrail.GetComponentsInChildren<ACC_Cable>(), cablesTrail.GetComponentsInChildren<ACC_CableJoint>(), cablesTrail.transform, cablesTrail.savePath, cablesTrail.generateBackside, cablesTrail.backsideDistance, cablesTrail.generateLightmapUVs);
                            combinePressed = true;
                            break;

                        case false:
                            break;
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(15);
            }

            //EDITOR PERFORMANCE
            editorPerformanceProperties = EditorGUILayout.Foldout(editorPerformanceProperties, "EDITOR PERFORMANCE", true);
            if (editorPerformanceProperties)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(showMeshes, new GUIContent("Show Meshes", "Disable on edit time if you have low fps."));
                GUILayout.EndVertical();
                GUILayout.Space(15);
            }


            EditorGUI.BeginDisabledGroup(cablesTrail.controlPoints.Count >= 2);

            if (cablesTrail.controlPoints.Count >= 2)
            {
                GUILayout.Label("'EXTERNAL CONTROL POINTS' used to control the trail segments amount and positions. \nSet the 'Points' array length to 0 to be able to control it manually.", EditorStyles.wordWrappedMiniLabel);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Add New Trail Section", "Add a new cable section after the latest one."), GUILayout.MaxWidth(180)))
            {
                cablesTrail.AddSegment();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUI.EndDisabledGroup();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            int trisCount = 0;
            for (int i = 0; i < cablesTrail.cableSequences.Count; i++)
            {
                for (int j = 0; j < cablesTrail.cableSequences[i].cables.Count; j++)
                {
                    if(cablesTrail.cableSequences[i].cables[j].tris != null)
                        trisCount += cablesTrail.cableSequences[i].cables[j].tris.Length / 3;
                }
            }
            GUILayout.Label("Total Triangles Count: " + trisCount);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (!combinePressed && !amountPressed)
                {
                    Undo.RecordObject(cablesTrail, cablesTrail.name + " change");

                    cablesTrail.SwitchShowMeshes();
                    cablesTrail.UpdateSequencesPositions();

                    cablesTrail.UpdateUVs();
                    cablesTrail.UpdateHeightScale();
                    cablesTrail.UpdateAllSequences();
                }
                combinePressed = false;
                amountPressed = false;
            }
        }

        private void SceneViewHandles(SceneView sceneView)
        {
            if (cablesTrail == null || !cablesTrail.enabled || !Selection.Contains(cablesTrail.gameObject)) return;

            //lock rotation and scale of the object
            cablesTrail.transform.eulerAngles = Vector3.zero;
            cablesTrail.transform.localScale = Vector3.one;

            if (cablesTrail.controlPoints != null && cablesTrail.controlPoints.Count < 2)
            {
                if (cablesTrail.prevPosStart.Length != cablesTrail.controlPointsStart.Count) return;

                Undo.RecordObject(cablesTrail, cablesTrail.name + " change");

                Handles.matrix = cablesTrail.transform.localToWorldMatrix;

                for (int i = 0; i < cablesTrail.controlPointsStart.Count; i++)
                {
                    if (i < 1)
                    {
                        cablesTrail.controlPointsStart[i] = Handles.PositionHandle(cablesTrail.controlPointsStart[i], Quaternion.identity);
                    }
                    cablesTrail.controlPointsEnd[i] = Handles.PositionHandle(cablesTrail.controlPointsEnd[i], Quaternion.identity);
                }

                for (int i = 0; i < cablesTrail.controlPointsStart.Count; i++)
                {
                    if (cablesTrail.controlPointsStart[i] != cablesTrail.prevPosStart[i] || cablesTrail.controlPointsEnd[i] != cablesTrail.prevPosEnd[i])
                    {
                        cablesTrail.UpdateSequencesPositions();
                        break;
                    }
                }

                cablesTrail.UpdatePrevPoints();
            }
        }
    }
}
#endif
