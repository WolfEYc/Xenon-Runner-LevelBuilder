
namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    public class ACC_Utilities : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Tools/NOT_Lonely/Advanced Cable Creator/Update all Cables", false, 12)]
        public static void UpdateAllCables()
        {
            ACC_Trail[] cableTrails = FindObjectsOfType<ACC_Trail>();

            for (int i = 0; i < cableTrails.Length; i++)
            {
                cableTrails[i].SetControlPoints();
                cableTrails[i].UpdateSequencesPositions();
                cableTrails[i].UpdateAllSequences();
            }
        }

        /// <summary>
        /// Get the tool root directory path
        /// </summary>
        public static string GetToolRootPath(MonoBehaviour script)
        {
            var thisScript = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(script));
            string toolRootFolder = Path.GetDirectoryName(thisScript);
            toolRootFolder = toolRootFolder.Replace('\\', '/');
            toolRootFolder = toolRootFolder.Replace(thisScript + ".cs", "");

            return toolRootFolder;
        }

        /// <summary>
        /// Get any texture of a provided material
        /// </summary>
        public static Texture GetTex(Material material)
        {
            Texture tex = material.mainTexture;

            //if the main texture is null then try to find any other texture
            if (tex == null)
            {
                if (material.HasProperty("_BumpMap")) tex = material.GetTexture("_BumpMap");
            }
            if (tex == null)
            {
                if (material.HasProperty("_NormalMap")) tex = material.GetTexture("_NormalMap");
            }
            if (tex == null)
            {
                if (material.HasProperty("_MetallicGlossMap")) tex = material.GetTexture("_MetallicGlossMap");
            }
            if (tex == null)
            {
                if (material.HasProperty("_SpecGlossMap")) tex = material.GetTexture("_SpecGlossMap");
            }
            if (tex == null)
            {
                if (material.HasProperty("_MaskMap")) tex = material.GetTexture("_MaskMap");
            }

            return tex;
        }
#endif
    }
}
