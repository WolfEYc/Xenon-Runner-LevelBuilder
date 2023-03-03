#if UNITY_EDITOR
namespace NOT_Lonely
{
    using UnityEngine;
    using UnityEditor;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ACC_Cable))]
    public class ACC_Cable_editor : Editor
    {
        SerializedProperty meshSettingsOverride;

        SerializedProperty radialSegments;
        SerializedProperty angle;
        SerializedProperty lengthSegments;
        SerializedProperty invert;
        SerializedProperty vertexAlphaBrightness;
        SerializedProperty textureTilingMultiplier;
        SerializedProperty uvsAngle;
        SerializedProperty uvTwist;
        SerializedProperty material;

        SerializedProperty shapeSettingsOverride;

        SerializedProperty heightScale;
        SerializedProperty lengthDependentHeight;
        SerializedProperty thickness;
        SerializedProperty useCurvature;
        SerializedProperty horizontalCurvature;
        SerializedProperty verticalCurvature;
        SerializedProperty noise;

        SerializedProperty jointSettingsOverride;

        SerializedProperty jointObj;
        SerializedProperty jointRotation;
        SerializedProperty jointObjScale;
        SerializedProperty uniformScale;
        SerializedProperty scale;

        private bool meshProperties = true;
        private bool shapeProperties = true;
        private bool jointProperties = true;

        private Texture2D icon;

        ACC_Cable cable;

        private string overrideSettingsTooltip = "If checked, then the settings below will be used instead of Cable Sequence settings for this category.";

        private void OnEnable()
        {
            meshSettingsOverride = serializedObject.FindProperty("meshSettingsOverride");

            radialSegments = serializedObject.FindProperty("radialSegments");
            angle = serializedObject.FindProperty("angle");
            lengthSegments = serializedObject.FindProperty("lengthSegments");
            invert = serializedObject.FindProperty("invert");
            vertexAlphaBrightness = serializedObject.FindProperty("vertexAlphaBrightness");
            textureTilingMultiplier = serializedObject.FindProperty("textureTilingMultiplier");
            uvsAngle = serializedObject.FindProperty("uvsAngle");
            uvTwist = serializedObject.FindProperty("uvTwist");
            material = serializedObject.FindProperty("material");

            shapeSettingsOverride = serializedObject.FindProperty("shapeSettingsOverride");

            heightScale = serializedObject.FindProperty("heightScale");
            lengthDependentHeight = serializedObject.FindProperty("lengthDependentHeight");
            thickness = serializedObject.FindProperty("thickness");
            useCurvature = serializedObject.FindProperty("useCurvature");
            horizontalCurvature = serializedObject.FindProperty("horizontalCurvature");
            verticalCurvature = serializedObject.FindProperty("verticalCurvature");
            noise = serializedObject.FindProperty("noise");

            jointSettingsOverride = serializedObject.FindProperty("jointSettingsOverride");

            jointObj = serializedObject.FindProperty("jointObj");
            jointRotation = serializedObject.FindProperty("jointRotation");
            jointObjScale = serializedObject.FindProperty("jointObjScale");
            uniformScale = serializedObject.FindProperty("uniformScale");
            scale = serializedObject.FindProperty("scale");
        }

        public static void CreateCable()
        {
            ACC_Cable cableObj = new GameObject("Cable", typeof(ACC_Cable)).GetComponent<ACC_Cable>();
            cableObj.name = "ACC_Cable";

            SceneView viewport = SceneView.lastActiveSceneView;

            cableObj.transform.position = viewport.camera.transform.position + (viewport.camera.transform.forward);
            Selection.activeObject = cableObj;

            cableObj.cableStart = cableObj.transform.position;
            cableObj.cableEnd = cableObj.transform.position + Vector3.forward;
            cableObj.ReDraw();
        }

        public override void OnInspectorGUI()
        {
            cable = target as ACC_Cable;
            if (cable == null || !cable.enabled) return;

            EditorGUI.BeginChangeCheck();
            icon = (Texture2D)AssetDatabase.LoadAssetAtPath(cable.toolRootFolder + "/UI/Icon_cable.png", typeof(Texture2D));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(icon);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("Go to the Cable Sequence", "Press this button to quickly select the Cable Sequence object this Cable belongs.")))
            {
                Selection.activeGameObject = cable.transform.parent.gameObject;
            }
            GUILayout.Space(15);

            //MESH 
            GUILayout.BeginHorizontal();
            meshProperties = EditorGUILayout.Foldout(meshProperties, new GUIContent("MESH", overrideSettingsTooltip), true);
            EditorGUILayout.PropertyField(meshSettingsOverride, GUIContent.none);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (meshProperties)
            {
                EditorGUI.BeginDisabledGroup(!cable.meshSettingsOverride);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(radialSegments, new GUIContent("Radial Segments", "Cross section segments on each cable/rope mesh."));
                cable.radialSegments = Mathf.Max(2, cable.radialSegments);

                EditorGUILayout.PropertyField(angle, new GUIContent("Angle", "A normalized angle of the cross section. \n Useful to rotate the mesh cross section when using only 2 'Radial Segments'."));
                cable.angle = Mathf.Clamp(cable.angle, 0, 1);

                EditorGUILayout.PropertyField(lengthSegments, new GUIContent("Length Segments", "Lengthwise segments count of each half of cable/rope mesh (total lengthwise segments = Length Segments * 2)"));
                cable.lengthSegments = Mathf.Max(4, cable.lengthSegments);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(invert, new GUIContent("Invert", "Inverts the mesh so you will see it from inside."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(vertexAlphaBrightness, new GUIContent("Vertex Alpha Brightness", "The brightness of the vertex a-channel, could be used for the vertex animation on a shader side."));
                GUILayout.EndVertical();
                cable.vertexAlphaBrightness = Mathf.Clamp01(cable.vertexAlphaBrightness);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(textureTilingMultiplier, new GUIContent("Texture Tiling Multiplier", "Stretch or squeeze mesh UVs: \n1 = original texture proportions, \n<1 = stretched texture, \n>1 = squeezed texture."));
                cable.textureTilingMultiplier = Mathf.Max(0.001f, cable.textureTilingMultiplier);

                EditorGUI.BeginDisabledGroup(!cable.isSquareTex);
                EditorGUILayout.PropertyField(uvsAngle, new GUIContent("UVs angle", "The angle of the generated UVs. Use this to rotate the texture. \nOnly available when a square texture in use."));
                cable.uvsAngle = Mathf.Clamp(cable.uvsAngle, 0, 360);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(uvTwist, new GUIContent("Texture Twist", "Useful to create a twisted cables look."));

                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(material, new GUIContent("Material", "Set this material to the mesh. \nIf this slot is empty, then a default material with a default shader will be set."));
                GUILayout.EndVertical();

                GUILayout.Space(15);
                EditorGUI.EndDisabledGroup();
            }

            //SHAPE  
            GUILayout.BeginHorizontal();
            shapeProperties = EditorGUILayout.Foldout(shapeProperties, new GUIContent("SHAPE", overrideSettingsTooltip), true);
            EditorGUILayout.PropertyField(shapeSettingsOverride, GUIContent.none);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (shapeProperties)
            {
                EditorGUI.BeginDisabledGroup(!cable.shapeSettingsOverride);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(heightScale, new GUIContent("Height Scale", "The cable/rope sagging."));

                EditorGUILayout.PropertyField(lengthDependentHeight, new GUIContent("Length Dependency", "Values above 0 will add a dependency of the cable section height scale to the length of the cable."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(thickness, new GUIContent("Thickness", "The thickness of the cable/rope mesh."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                /*
                EditorGUILayout.PropertyField(useCurvature, new GUIContent("Curvature", "Add an extra curvature to the ends of the cable."));
                */
                EditorGUI.BeginDisabledGroup(!cable.useCurvature);
                EditorGUILayout.PropertyField(horizontalCurvature, new GUIContent("Horizontal Curvature", "The amount of curvature on the joints of the cable in the horizontal plane."));
                EditorGUILayout.PropertyField(verticalCurvature, new GUIContent("Vertical Curvature", "The amount of curvature on the joints of the cable in the vertical plane."));
                EditorGUI.EndDisabledGroup();
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(noise, new GUIContent("Noise", "Amount of position randomness applied to the cable vertices."));
                GUILayout.EndVertical();

                cable.lengthDependentHeight = Mathf.Max(0, cable.lengthDependentHeight);
                cable.thickness = Mathf.Max(0.001f, cable.thickness);
                cable.horizontalCurvature = Mathf.Max(0, cable.horizontalCurvature);
                cable.verticalCurvature = Mathf.Max(0, cable.verticalCurvature);
                cable.noise = Mathf.Max(0, cable.noise);


                GUILayout.Space(15);
                EditorGUI.EndDisabledGroup();
            }

            //JOINT
            GUILayout.BeginHorizontal();
            jointProperties = EditorGUILayout.Foldout(jointProperties, new GUIContent("JOINT", overrideSettingsTooltip), true);
            EditorGUILayout.PropertyField(jointSettingsOverride, GUIContent.none);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (jointProperties)
            {
                EditorGUI.BeginDisabledGroup(!cable.jointSettingsOverride);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(jointObj, new GUIContent("Joint Object", "This object will be placed on the cable joint. Put a prefab from the Project folder here."));
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Update Joint Object", "Press this button to update the joint object, if you added a new one into the slot."), GUILayout.MaxWidth(128)))
                {
                    cable.SetJointObject();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(jointRotation, new GUIContent("Rotation", "Local rotation of the joint. \nAdjust it if your joint object oriented wrong."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(uniformScale, new GUIContent("Use Uniform Scale", "If checked, then joint object will be scaled uniformly by all axis."));

                if (cable.uniformScale)
                {
                    EditorGUILayout.PropertyField(scale, new GUIContent("Scale", "The scale multiplier for the joint object, relative to the cable/rope thickness."));
                    cable.jointObjScale = Vector3.one * cable.scale;
                }
                else
                {
                    EditorGUILayout.PropertyField(jointObjScale, new GUIContent("Scale", "The scale multiplier for the joint object, relative to the cable/rope thickness."));
                }
                GUILayout.EndVertical();

                GUILayout.Space(15);
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Current Cable Triangles Count: " + cable.tris.Length / 3);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (cable.startPoint) cable.cableStart = cable.startPoint.position;
                if (cable.endPoint) cable.cableEnd = cable.endPoint.position;

                if (!cable._jointObj) cable.SetJointObject();

                cable.UpdateJointObjectRotation();
                cable.SetMaterial();

                cable.ReDraw();
            }
        }
    }
}
#endif
