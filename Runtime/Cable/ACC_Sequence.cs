namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.IO;

    [ExecuteInEditMode]
    public class ACC_Sequence : MonoBehaviour
    {
        public Vector3 pointsOffset = Vector3.zero;

        //[Header("MESH")]
        public int radialSegments = 6;
        public float angle = 0;
        public int lengthSegments = 10;
        public bool invert = false;
        public float vertexAlphaBrightness = 0;
        public float textureTilingMultiplier = 1f;
        public int uvsAngle = 0;
        public float uvTwist = 0;
        public bool isSquareTex;
        public Material material;

        //[Header("SHAPE")]
        public Vector2 heightScale = new Vector2(0.1f, 0.1f);
        public float lengthDependentHeight = 0.5f;
        public float thickness = 0.05f;
        public float horizontalCurvature = 1;
        public float verticalCurvature = 1;
        public float noise = 0;

        //[Header("JOINT")]
        public Transform jointObj;
        public Vector3 jointRotation = Vector3.zero;
        public Vector3 jointObjScale = Vector3.one;
        public bool uniformScale = true;
        public float scale = 1;

        [HideInInspector] public List<ACC_Cable> cables = new List<ACC_Cable>();

        private Vector3 currCableStart;
        private Vector3 currCableEnd;
        private Vector3 currCableDir;

        public bool meshSettingsOverride = true;
        public bool shapeSettingsOverride = true;
        public bool jointSettingsOverride = true;

        public string toolRootFolder;

        public delegate void CableRemoved(int cableID, ACC_Cable cable);
        public event CableRemoved OnAnyCableRemoved;
        
#if UNITY_EDITOR

        private void OnEnable()
        {
            if(Application.isPlaying) return;
            
            EditorApplication.update += OnEditorUpdate;
            
            toolRootFolder = ACC_Utilities.GetToolRootPath(this);
            TryGetDefaultMaterial();

            for (int i = 0; i < cables.Count; i++)
            {
                if (cables[i])
                {
                    cables[i].OnCableStartMoved -= OnCableStartMoved;
                    cables[i].OnCableEndMoved -= OnCableEndMoved;
                    cables[i].OnCableRemoved -= OnCableRemoved;

                    cables[i].OnCableStartMoved += OnCableStartMoved;
                    cables[i].OnCableEndMoved += OnCableEndMoved;
                    cables[i].OnCableRemoved += OnCableRemoved;
                }
            }

        }

        void TryGetDefaultMaterial()
        {
            string mtlPath = toolRootFolder + "/Materials/CableC.mat";
            if (!material) material = AssetDatabase.LoadAssetAtPath(mtlPath, typeof(Material)) as Material;
        }

        public void CreateNewCable(Transform managerTransform, int radialSegs, int lengthSegs, bool is1st = false)
        {
            ACC_Cable newCable = new GameObject("Cable", typeof(ACC_Cable)).GetComponent<ACC_Cable>();

            newCable.radialSegments = radialSegs;
            newCable.lengthSegments = lengthSegs;

            newCable.transform.parent = managerTransform;
            newCable.transform.localPosition = Vector3.zero;

            newCable.OnCableStartMoved += OnCableStartMoved;
            newCable.OnCableEndMoved += OnCableEndMoved;
            newCable.OnCableRemoved += OnCableRemoved;

            if (is1st)
            {
                newCable.cableStart = managerTransform.position;
                newCable.cableEnd = managerTransform.position + Vector3.forward;
            }
            else
            {
                ACC_Cable prevCable = cables[cables.Count - 1];
                newCable.cableStart = prevCable.cableEnd;
                newCable.cableEnd = prevCable.cableEnd + (prevCable.cableEnd - prevCable.cableStart).normalized;
            }

            newCable.meshSettingsOverride = false;
            newCable.shapeSettingsOverride = false;
            newCable.jointSettingsOverride = false;

            newCable.ResetPivot();

            cables.Add(newCable);
        }

        void OnCableStartMoved(ACC_Cable cable)
        {
            for (int i = 0; i < cables.Count; i++)
            {
                if (cables[i] == cable)
                {
                    ACC_Cable nextCable;
                    ACC_Cable prevCable;
                    ACC_Cable extraCable;

                    nextCable = i != cables.Count - 1 ? cables[i + 1] : null;
                    prevCable = i != 0 ? cables[i - 1] : null;

                    if (cables.Count >= 3)
                    {
                        extraCable = i >= 2 ? cables[i - 2] : null;
                    }
                    else
                    {
                        extraCable = null;
                    }

                    if (prevCable) prevCable.cableEnd = cable.cableStart;

                    cable.ResetPivot();
                    if (prevCable)
                    {
                        prevCable.ResetPivot();
                        prevCable.RecalculateJointObject();
                        prevCable.ReDraw();
                    }

                    UpdateCableJoints(cable, prevCable, nextCable, extraCable, i, true);

                    break;
                }
            }
        }

        void OnCableEndMoved(ACC_Cable cable)
        {
            for (int i = 0; i < cables.Count; i++)
            {
                if (cables[i] == cable)
                {
                    ACC_Cable nextCable;
                    ACC_Cable prevCable;
                    ACC_Cable extraCable = null;

                    nextCable = i < cables.Count - 1 ? cables[i + 1] : null;
                    prevCable = i != 0 ? cables[i - 1] : null;

                    if (cables.Count >= 3)
                    {
                        extraCable = i < cables.Count - 2 ? cables[i + 2] : null;
                    }
                    else
                    {
                        extraCable = null;
                    }

                    if (nextCable) nextCable.cableStart = cable.cableEnd;

                    cable.ResetPivot();
                    if (nextCable)
                    {
                        nextCable.ResetPivot();
                        nextCable.RecalculateJointObject();
                        nextCable.ReDraw();
                    }

                    UpdateCableJoints(cable, prevCable, nextCable, extraCable, i, false);

                    break;
                }
            }
        }

        ACC_Cable curCable;
        void UpdateCableJoints(ACC_Cable currCable, ACC_Cable prevCable, ACC_Cable nextCable, ACC_Cable extraCable, int i, bool isStart)
        {
            curCable = currCable;
            currCableStart = currCable.cableStart;
            currCableEnd = currCable.cableEnd;
            currCableDir = (currCable.cableEnd - currCable.cableStart);

            ///////////////////////////////
            #region Update Curvatures

            currCable.endCurveSideModifier = 0;
            curCable.startCurveSideModifier = 0;

            if (nextCable)
            {
                float offset;
                Vector3 cablesNormal;
                CalculateCurveOffset(currCable.cableStart, currCable.cableEnd, nextCable.cableEnd, out offset, out cablesNormal);

                currCable.endCurveSideModifier = offset;
                nextCable.startCurveSideModifier = offset;

                nextCable.UpdateJointObjectRotation(Quaternion.LookRotation(cablesNormal, Vector3.up));
            }

            if (prevCable)
            {
                float offset;
                Vector3 cablesNormal;
                CalculateCurveOffset(currCableEnd, currCableStart, prevCable.cableStart, out offset, out cablesNormal);

                prevCable.endCurveSideModifier = -offset;
                curCable.startCurveSideModifier = -offset;

                curCable.UpdateJointObjectRotation(Quaternion.LookRotation(cablesNormal, Vector3.up));
            }

            if (extraCable)
            {
                if (isStart)
                {
                    float offset;
                    Vector3 cablesNormal;
                    CalculateCurveOffset(prevCable.cableEnd, extraCable.cableEnd, extraCable.cableStart, out offset, out cablesNormal);

                    prevCable.startCurveSideModifier = -offset;
                    extraCable.endCurveSideModifier = -offset;

                    prevCable.UpdateJointObjectRotation(Quaternion.LookRotation(cablesNormal, Vector3.up));
                }
                else
                {
                    float offset;
                    Vector3 cablesNormal;
                    CalculateCurveOffset(nextCable.cableStart, extraCable.cableStart, extraCable.cableEnd, out offset, out cablesNormal);

                    nextCable.endCurveSideModifier = offset;
                    extraCable.startCurveSideModifier = offset;

                    extraCable.UpdateJointObjectRotation(Quaternion.LookRotation(cablesNormal, Vector3.up));
                }
            }

            prevCable?.ReDraw();
            curCable?.ReDraw();
            nextCable?.ReDraw();
            extraCable?.ReDraw();
        }

        #endregion

        void CalculateCurveOffset(Vector3 cableA_start, Vector3 cableA_end, Vector3 cableB_end, out float offset, out Vector3 cablesNormal)
        {
            Vector3 cablesDir = Vector3.ProjectOnPlane((cableB_end - cableA_start), Vector3.up);

            Vector3 projectedPoint = Vector3.Project(cableA_end - cableA_start, cablesDir) + cableA_start;
            projectedPoint.y = cableA_end.y;

            Vector3 cross = Vector3.Cross(cablesDir.normalized, (cableA_end - cableA_start).normalized);

            offset = Mathf.Sign(cross.y) * Vector3.Distance(projectedPoint, cableA_end);

            float dot = Vector3.Dot(Vector3.ProjectOnPlane(cableA_start - cableA_end, Vector3.up).normalized, Vector3.ProjectOnPlane(cableB_end - cableA_end, Vector3.up).normalized);

            Vector3 averagePoint = (cableB_end + cableA_start) / 2;
            Vector3 dirToAverage = cableA_end - averagePoint;
            dirToAverage = Vector3.ProjectOnPlane(dirToAverage.normalized, Vector3.up).normalized;

            dirToAverage = Quaternion.AngleAxis(90, Vector3.up) * dirToAverage.normalized;

            if (cross.y > 0)
            {
                dirToAverage = -dirToAverage;
            }

            float t = 0.5f + Mathf.Clamp(-dot, 0, 1);

            cablesNormal = Vector3.Lerp(dirToAverage, cablesDir.normalized, t).normalized;
        }

        void OnCableRemoved(ACC_Cable cable)
        {
            cable.OnCableStartMoved -= OnCableStartMoved;
            cable.OnCableEndMoved -= OnCableEndMoved;

            for (int i = 0; i < cables.Count; i++)
            {
                if (cable == cables[i])
                {
                    OnAnyCableRemoved?.Invoke(i, cable);
                    break;
                }
            }
            cables.Remove(cable);
        }

        public void Unsubscribe(int cableID)
        {
            cables[cableID].OnCableRemoved -= OnCableRemoved;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;

            for (int i = 0; i < cables.Count; i++)
            {
                if (cables[i])
                {
                    cables[i].OnCableRemoved -= OnCableRemoved;
                }
            }
        }

        Vector2 prevHeightScale;
        public void UpdateAllCables(bool isSequenceEditorCall)
        {
            if (material)
            {
                Texture tex = ACC_Utilities.GetTex(material);
                if (tex != null)
                {
                    isSquareTex = tex.height == tex.width;
                }
            }

            for (int i = 0; i < cables.Count; i++)
            {
                cables[i].pointsOffset = pointsOffset;

                if (!cables[i].meshSettingsOverride)
                {
                    if (!meshSettingsOverride || isSequenceEditorCall)
                    {
                        cables[i].radialSegments = radialSegments;
                        cables[i].angle = angle;
                        cables[i].lengthSegments = lengthSegments;
                        cables[i].invert = invert;
                        cables[i].vertexAlphaBrightness = vertexAlphaBrightness;
                        cables[i].textureTilingMultiplier = textureTilingMultiplier;
                        cables[i].uvsAngle = uvsAngle;
                        cables[i].uvTwist = uvTwist;
                        cables[i].uvTwistOffset = i == 0 ? 0 : cables[i - 1].hStep;
                        cables[i].material = material;
                        cables[i].SetMaterial();
                    }
                }

                if (!cables[i].shapeSettingsOverride)
                {
                    if (!shapeSettingsOverride || isSequenceEditorCall)
                    {
                        cables[i].thickness = thickness;
                        /*if (heightScale != prevHeightScale)*/
                        cables[i].heightScale = Random.Range(heightScale.x, heightScale.y);
                        cables[i].lengthDependentHeight = lengthDependentHeight;
                        cables[i].verticalCurvature = verticalCurvature;
                        cables[i].horizontalCurvature = horizontalCurvature;
                        cables[i].noise = noise;
                    }
                }

                if (!cables[i].jointSettingsOverride)
                {
                    if (!jointSettingsOverride || isSequenceEditorCall)
                    {
                        cables[i].jointObj = jointObj;
                        cables[i].jointRotation = jointRotation;
                        cables[i].jointObjScale = jointObjScale;
                        cables[i].UpdateJointObjectRotation();

                        if (!cables[i]._jointObj) cables[i].SetJointObject();
                    }
                }

                if (!cables[i].meshSettingsOverride || !cables[i].shapeSettingsOverride || !cables[i].jointSettingsOverride)
                {
                    cables[i].ReDraw();
                }
            }
            prevHeightScale = heightScale;
        }
        public void UpdateUVs(float offset)
        {
            for (int i = 0; i < cables.Count; i++)
            {
                cables[i].uvOffset = offset;
                //cables[i].ReDraw();
            }
        }

        public void SetJointObjects()
        {
            for (int i = 0; i < cables.Count; i++)
            {
                cables[i].SetJointObject();
            }
        }

        void OnEditorUpdate()
        {
            if (Application.isPlaying) return;
            if (!Selection.Contains(gameObject)) return;

            if (meshSettingsOverride || shapeSettingsOverride || jointSettingsOverride)
            {
                if (gameObject.name != "Cable Sequence (Overrides are used)")
                {
                    gameObject.name = "Cable Sequence (Overrides are used)";
                }
            }
            else
            {
                if (gameObject.name != "Cable Sequence")
                {
                    gameObject.name = "Cable Sequence";
                }
            }

            transform.position = transform.parent.position;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
#endif
    }
}

