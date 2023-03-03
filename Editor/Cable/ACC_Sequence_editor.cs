#if UNITY_EDITOR
namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ACC_Sequence))]
    public class ACC_Sequence_editor : Editor
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

        private string overrideSettingsTooltip = "If checked, then the settings below will be used instead of Cable Trail settings for this category.";

        private Texture2D icon;

        ACC_Sequence cableSequence;

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

        public override void OnInspectorGUI()
        {
            cableSequence = target as ACC_Sequence;

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);

            icon = (Texture2D)AssetDatabase.LoadAssetAtPath(cableSequence.toolRootFolder + "/UI/Icon_sequence.png", typeof(Texture2D));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(icon);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent("Go to the Cable Trail", "Press this button to quickly select the Cable Trail object in the hierarchy.")))
            {
                Selection.activeGameObject = cableSequence.transform.parent.gameObject;
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
                EditorGUI.BeginDisabledGroup(!cableSequence.meshSettingsOverride);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(radialSegments, new GUIContent("Radial Segments", "Cross section segments on each cable/rope mesh."));
                cableSequence.radialSegments = Mathf.Max(2, cableSequence.radialSegments);

                EditorGUILayout.PropertyField(angle, new GUIContent("Angle", "A normalized angle of the cross section. \n Useful to rotate the mesh cross section when using only 2 'Radial Segments'."));
                cableSequence.angle = Mathf.Clamp(cableSequence.angle, 0, 1);

                EditorGUILayout.PropertyField(lengthSegments, new GUIContent("Length Segments", "Lengthwise segments count of each half of cable/rope mesh (total lengthwise segments = Length Segments * 2)"));
                cableSequence.lengthSegments = Mathf.Max(4, cableSequence.lengthSegments);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(invert, new GUIContent("Invert", "Inverts the mesh so you will see it from inside."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(vertexAlphaBrightness, new GUIContent("Vertex Alpha Brightness", "The brightness of the vertex a-channel, could be used for the vertex animation on a shader side."));
                GUILayout.EndVertical();
                cableSequence.vertexAlphaBrightness = Mathf.Clamp01(cableSequence.vertexAlphaBrightness);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(textureTilingMultiplier, new GUIContent("Texture Tiling Multiplier", "Stretch or squeeze mesh UVs: \n1 = original texture proportions, \n<1 = stretched texture, \n>1 = squeezed texture."));
                cableSequence.textureTilingMultiplier = Mathf.Max(0.001f, cableSequence.textureTilingMultiplier);

                EditorGUI.BeginDisabledGroup(!cableSequence.isSquareTex);
                EditorGUILayout.PropertyField(uvsAngle, new GUIContent("UVs angle", "The angle of the generated UVs. Use this to rotate the texture. \nOnly available when a square texture in use."));
                cableSequence.uvsAngle = Mathf.Clamp(cableSequence.uvsAngle, 0, 360);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(uvTwist, new GUIContent("Texture Twist", "Useful to create a twisted cables look."));

                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(material, new GUIContent("Material", "Set this material to each mesh in the trail. \nIf this slot is empty, then a default material with a default shader will be set."));
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
                EditorGUI.BeginDisabledGroup(!cableSequence.shapeSettingsOverride);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(heightScale, new GUIContent("Height Scale min/max", "The cable sagging. \nUse different values for the 'X'(min) and 'Y'(max) to randomize."));
                EditorGUILayout.PropertyField(lengthDependentHeight, new GUIContent("Length Dependency", "Values above 0 will add a dependency of the cable section height scale to the length of the cable."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(thickness, new GUIContent("Thickness", "The thickness of the cable/rope mesh."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(horizontalCurvature, new GUIContent("Horizontal Curvature", "The amount of curvature on the joints of the cable in the horizontal plane."));
                EditorGUILayout.PropertyField(verticalCurvature, new GUIContent("Vertical Curvature", "The amount of curvature on the joints of the cable in the vertical plane."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(noise, new GUIContent("Noise", "Amount of position randomness applied to the cable vertices."));
                GUILayout.EndVertical();

                cableSequence.thickness = Mathf.Max(0.001f, cableSequence.thickness);
                cableSequence.lengthDependentHeight = Mathf.Max(0, cableSequence.lengthDependentHeight);
                cableSequence.horizontalCurvature = Mathf.Max(0, cableSequence.horizontalCurvature);
                cableSequence.verticalCurvature = Mathf.Max(0, cableSequence.verticalCurvature);
                cableSequence.noise = Mathf.Max(0, cableSequence.noise);


                GUILayout.Space(15);
                EditorGUI.EndDisabledGroup();
            }

            //JOINTS
            GUILayout.BeginHorizontal();
            jointProperties = EditorGUILayout.Foldout(jointProperties, new GUIContent("JOINTS", overrideSettingsTooltip), true);
            EditorGUILayout.PropertyField(jointSettingsOverride, GUIContent.none);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (jointProperties)
            {
                EditorGUI.BeginDisabledGroup(!cableSequence.jointSettingsOverride);

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(jointObj, new GUIContent("Joint Object", "This object will be placed on the cable joint. Put a prefab from the Project folder here."));
                GUILayout.Space(10);
                if (GUILayout.Button(new GUIContent("Update Joint Object", "Press this button to update the joint object, if you added a new one into the slot."), GUILayout.MaxWidth(128)))
                {
                    cableSequence.SetJointObjects();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(jointRotation, new GUIContent("Rotation", "Local rotation of the joints. \nAdjust it if your joint objects oriented wrong."));
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(uniformScale, new GUIContent("Use Uniform Scale", "If checked, then joint object will be scaled uniformly by all axis."));

                if (cableSequence.uniformScale)
                {
                    EditorGUILayout.PropertyField(scale, new GUIContent("Scale", "The scale multiplier for the joint object, relative to the cable/rope thickness."));
                    cableSequence.jointObjScale = Vector3.one * cableSequence.scale;
                }
                else
                {
                    EditorGUILayout.PropertyField(jointObjScale, new GUIContent("Scale", "The scale multiplier for the joint object, relative to the cable/rope thickness."));
                }
                GUILayout.EndVertical();

                GUILayout.Space(15);
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.Space(5);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            int trisCount = 0;
            for (int i = 0; i < cableSequence.cables.Count; i++)
            {
                trisCount += cableSequence.cables[i].tris.Length / 3;
            }
            GUILayout.Label("Current Sequence Triangles Count: " + trisCount);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                cableSequence.UpdateAllCables(true);
            }
        }
    }
}
#endif
