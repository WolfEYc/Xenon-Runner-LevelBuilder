
namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Linq;

    public class ACC_combiner : MonoBehaviour
    {
#if UNITY_EDITOR
        public static void Combine(ACC_Cable[] cables, ACC_CableJoint[] joints, Transform root, string savePath, bool doubleSided, float offset, bool generateLightmapUVs)
        {
            int count = 0;

            //unpack prefab if it's exist
            if (PrefabUtility.IsAnyPrefabInstanceRoot(root.gameObject)) PrefabUtility.UnpackPrefabInstance(root.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            //disable the root object
            root.gameObject.SetActive(false);

            //create a name for the final combined object
            string combinedObjectName = root.name + "_combined";

            List<GameObject> subObjects = new List<GameObject>();
            List<MeshRenderer> cablesRenderers = new List<MeshRenderer>();
            List<MeshFilter> cablesFilters = new List<MeshFilter>();

            //List<Vector3> allVertices = new List<Vector3>();

            for (int i = 0; i < cables.Length; i++)
            {
                cablesRenderers.Add(cables[i].GetComponent<MeshRenderer>());
                cablesFilters.Add(cables[i].GetComponent<MeshFilter>());
                count++;
            }

            //create a list of all objects including the joint objects and pre-combined cables
            List<MeshRenderer> allRenderers = new List<MeshRenderer>();
            List<MeshFilter> allFilters = new List<MeshFilter>();

            for (int i = 0; i < joints.Length; i++)
            {
                Transform[] allChildren = joints[i].GetComponentsInChildren<Transform>();
                for (int j = 0; j < allChildren.Length; j++)
                {
                    MeshFilter filter = allChildren[j].GetComponent<MeshFilter>();

                    if (filter)
                    {
                        allRenderers.Add(allChildren[j].GetComponent<MeshRenderer>());
                        allFilters.Add(filter);
                        count++;
                    }
                }
                if (PrefabUtility.IsAnyPrefabInstanceRoot(joints[i].gameObject)) PrefabUtility.UnpackPrefabInstance(joints[i].gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            GameObject combinedCable;
            GameObject combinedSideA;
            GameObject combinedSideB;
            GameObject precombinedCable = null;

            //combine cables
            CombineMeshes(root, cablesRenderers, cablesFilters, doubleSided, out combinedSideA, out combinedSideB, offset);

            if (doubleSided)
            {
                List<MeshRenderer> cableSidesRenderers = new List<MeshRenderer>();
                List<MeshFilter> cableSidesFilters = new List<MeshFilter>();

                cableSidesRenderers.Add(combinedSideA.GetComponent<MeshRenderer>());
                cableSidesRenderers.Add(combinedSideB.GetComponent<MeshRenderer>());

                cableSidesFilters.Add(combinedSideA.GetComponent<MeshFilter>());
                cableSidesFilters.Add(combinedSideB.GetComponent<MeshFilter>());

                CombineMeshes(root, cableSidesRenderers, cableSidesFilters, false, out precombinedCable, out _);

                DestroyImmediate(combinedSideA);
                DestroyImmediate(combinedSideB);

                //add the pre-combined cable to the common objects list
                Mesh mesh = precombinedCable.GetComponent<MeshFilter>().sharedMesh;
                if (mesh.subMeshCount > 1)
                {
                    for (int i = 0; i < mesh.subMeshCount; i++)
                    {
                        GameObject subGO = new GameObject("SubGO " + i);
                        subGO.transform.SetPositionAndRotation(root.transform.position, root.transform.rotation);
                        Mesh submesh = mesh.GetSubmesh(i);

                        MeshRenderer subRenderer = subGO.AddComponent<MeshRenderer>();
                        MeshFilter subFilter = subGO.AddComponent<MeshFilter>();

                        subFilter.sharedMesh = submesh;
                        subRenderer.sharedMaterial = precombinedCable.GetComponent<MeshRenderer>().sharedMaterials[i];

                        allRenderers.Add(subRenderer);
                        allFilters.Add(subFilter);

                        subObjects.Add(subGO);
                    }
                }
                else
                {
                    allRenderers.Add(precombinedCable.GetComponent<MeshRenderer>());
                    allFilters.Add(precombinedCable.GetComponent<MeshFilter>());
                }
            }
            else
            {
                //add the combined cable to the common objects list
                Mesh mesh = combinedSideA.GetComponent<MeshFilter>().sharedMesh;
                if (mesh.subMeshCount > 1)
                {
                    for (int i = 0; i < mesh.subMeshCount; i++)
                    {
                        GameObject subGO = new GameObject("SubGO " + i);
                        subGO.transform.SetPositionAndRotation(root.transform.position, root.transform.rotation);
                        Mesh submesh = mesh.GetSubmesh(i);

                        MeshRenderer subRenderer = subGO.AddComponent<MeshRenderer>();
                        MeshFilter subFilter = subGO.AddComponent<MeshFilter>();

                        subFilter.sharedMesh = submesh;
                        subRenderer.sharedMaterial = combinedSideA.GetComponent<MeshRenderer>().sharedMaterials[i];

                        allRenderers.Add(subRenderer);
                        allFilters.Add(subFilter);

                        subObjects.Add(subGO);
                    }
                }
                else
                {
                    allRenderers.Add(combinedSideA.GetComponent<MeshRenderer>());
                    allFilters.Add(combinedSideA.GetComponent<MeshFilter>());
                }
            }

            //final combine pre-combined cable with the joint objects
            CombineMeshes(root, allRenderers, allFilters, false, out combinedCable, out _);

            if (precombinedCable) DestroyImmediate(precombinedCable);
            if (combinedSideA) DestroyImmediate(combinedSideA);

            for (int i = 0; i < subObjects.Count; i++)
            {
                if (subObjects[i] != null) DestroyImmediate(subObjects[i]);
            }

            subObjects.Clear();

            Mesh combinedMesh = combinedCable.GetComponent<MeshFilter>().sharedMesh;

            if (generateLightmapUVs)
            {
                Unwrapping.GenerateSecondaryUVSet(combinedMesh);
            }
            else
            {
                combinedMesh.uv2 = null;
            }

            combinedCable.transform.SetSiblingIndex(root.GetSiblingIndex() + 1);
            combinedCable.isStatic = true;

            string combinedMeshesPath = savePath;
            Directory.CreateDirectory(combinedMeshesPath);

            AssetDatabase.CreateAsset(combinedCable.GetComponent<MeshFilter>().sharedMesh, combinedMeshesPath + "/" + root.name + "_combined_" + System.DateTime.Now.ToString("MMddyy-hhmmss") + ".asset");
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = combinedCable;

            Debug.Log(count + " meshes have been combined. " + "'<color=#40b8ff>" + combinedObjectName + "</color>' object created.");
        }

        static void CombineMeshes(Transform root, List<MeshRenderer> renderers, List<MeshFilter> filters, bool doubleSided, out GameObject combinedObject, out GameObject combinedObject_2side, float offset = 0)
        {
            //create an empty object with all the necessary components
            combinedObject = new GameObject(root.name + "_combined");
            MeshFilter combinedFilter = combinedObject.AddComponent<MeshFilter>();
            MeshRenderer combinedRenderer = combinedObject.AddComponent<MeshRenderer>();

            //reset its transform to the uncombined object transform
            combinedObject.transform.position = root.position;
            combinedObject.transform.rotation = root.rotation;

            //create component lists

            List<Material> materials = new List<Material>();

            //create the original parents list and reparent
            List<Transform> parents = new List<Transform>();
            for (int i = 0; i < filters.Count; i++)
            {
                parents.Add(filters[i].transform.parent);
                filters[i].transform.parent = combinedObject.transform;
            }

            combinedObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            //fill the cables materials list with the unique materials
            for (int i = 0; i < renderers.Count; i++)
            {
                Material[] mtls = renderers[i].sharedMaterials;
                for (int j = 0; j < mtls.Length; j++)
                {
                    if (!materials.Contains(mtls[j]))
                    {
                        materials.Add(mtls[j]);
                    }
                }
            }

            //sort the cables components by unique materials
            List<MeshRenderer>[] sortedRenderers = new List<MeshRenderer>[materials.Count];
            List<MeshFilter>[] sortedFilters = new List<MeshFilter>[materials.Count];

            for (int i = 0; i < materials.Count; i++)
            {
                sortedRenderers[i] = new List<MeshRenderer>();
                sortedFilters[i] = new List<MeshFilter>();

                for (int j = 0; j < renderers.Count; j++)
                {
                    if (renderers[j].sharedMaterial == materials[i])
                    {
                        sortedRenderers[i].Add(renderers[j]);
                        sortedFilters[i].Add(filters[j]);
                    }
                }
            }

            List<Mesh> meshes = new List<Mesh>();

            //combine meshes by unique materials and fill the 'meshes' list with them
            for (int i = 0; i < sortedRenderers.Length; i++)
            {
                List<CombineInstance> combinersList = new List<CombineInstance>();
                for (int j = 0; j < sortedRenderers[i].Count; j++)
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = sortedFilters[i][j].sharedMesh;
                    ci.subMeshIndex = 0;
                    ci.transform = sortedFilters[i][j].transform.localToWorldMatrix;

                    combinersList.Add(ci);
                }

                Mesh mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.CombineMeshes(combinersList.ToArray(), true);
                meshes.Add(mesh);
            }

            //combine all the meshes into a single mesh
            List<CombineInstance> finalCombiners = new List<CombineInstance>();
            for (int i = 0; i < meshes.Count; i++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = meshes[i];
                ci.subMeshIndex = 0;
                ci.transform = Matrix4x4.identity;
                finalCombiners.Add(ci);
            }

            Mesh finalMesh = new Mesh();
            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            finalMesh.CombineMeshes(finalCombiners.ToArray(), false);

            //create a back side of the mesh
            combinedObject_2side = null;
            if (doubleSided)
            {
                Mesh secondMesh = new Mesh();
                secondMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                Vector3[] vertices = finalMesh.vertices;
                Vector3[] normals = finalMesh.normals;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = vertices[i] + (normals[i] * -offset);
                }

                secondMesh.vertices = vertices;
                secondMesh.colors = finalMesh.colors;
                secondMesh.uv = finalMesh.uv;
                secondMesh.triangles = finalMesh.triangles.Reverse().ToArray();

                secondMesh.RecalculateNormals();
                secondMesh.RecalculateBounds();
                secondMesh.RecalculateTangents();

                combinedObject_2side = new GameObject("SecondSide");
                MeshFilter mf = combinedObject_2side.AddComponent<MeshFilter>();
                MeshRenderer mr = combinedObject_2side.AddComponent<MeshRenderer>();

                mf.sharedMesh = secondMesh;

                mr.sharedMaterials = materials.ToArray();

                combinedObject_2side.transform.SetPositionAndRotation(root.position, root.rotation);
            }

            combinedObject.transform.SetPositionAndRotation(root.position, root.rotation);

            //reparent objects back to their original parents
            for (int i = 0; i < filters.Count; i++)
            {
                filters[i].transform.parent = parents[i];
            }

            combinedFilter.sharedMesh = finalMesh;
            combinedRenderer.sharedMaterials = materials.ToArray();
        }
#endif
    }
}


