
namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using UnityEditor;

    [ExecuteInEditMode]
    public class ACC_Trail : MonoBehaviour
    {
        //public Transform[] controlPoints = new Transform[0];
        public List<Transform> controlPoints = new List<Transform>();
        public Vector3 pointsOffset = Vector3.zero;

        //[Header("ARRAY PROPERTIES")]
        public int amount = 1;
        public Vector3 stepSize = new Vector3(0.3f, 0, 0);
        public Vector3 positionRandomness = new Vector3(0, 0, 0);

        //[Header("MESH")]
        public int radialSegments = 6;
        public float angle = 0;
        public int lengthSegments = 10;
        public bool invert = false;
        public float vertexAlphaBrightness = 0;
        public float textureTilingMultiplier = 1f;
        public float textureOffsetRandomness = 1f;
        public int uvsAngle = 0;
        public Vector2 uvTwist;
        public Material[] materials = new Material[1];

        //[Header("SHAPE")]
        public Vector2 heightScale = new Vector2(0.1f, 0.1f);
        public float lengthDependentHeight = 0.5f;
        public Vector2 thickness = new Vector2(0.05f, 0.05f);
        public float horizontalCurvature = 1;
        public float verticalCurvature = 1;
        public float noise = 0;

        //[Header("JOINTS")]
        public Transform jointObj;
        public Vector3 jointRotation = Vector3.zero;
        public Vector3 jointObjScale = Vector3.one;
        public bool uniformScale = true;
        public float scale = 1;

        //OPTIMIZATION
        public bool generateLightmapUVs = true;
        public bool generateBackside = true;
        public float backsideDistance = 0.002f;
        public string savePath = "Assets/NOT_Lonely/Advanced Cable Creator/CombinedMeshes";

        //[Header("EDITOR PERFORMANCE")]
        public bool showMeshes = true;

        public List<ACC_Sequence> cableSequences = new List<ACC_Sequence>();

        public List<Vector3> controlPointsStart = new List<Vector3>();
        public List<Vector3> controlPointsEnd = new List<Vector3>();
        [HideInInspector] public Vector3[] prevPosStart;
        [HideInInspector] public Vector3[] prevPosEnd;

        private Vector3 sectionNormalV;
        private Vector3 sectionNormalH;

        private Vector3[] steps;
        private float[] stepX_sums;
        private float[] stepY_sums;
        private float[] stepZ_sums;
        private float[][] stepsX;
        private float[][] stepsY;
        private float[][] stepsZ;

        public string toolRootFolder;
#if UNITY_EDITOR

        private void OnEnable()
        {
            if(Application.isPlaying) return;
            
            toolRootFolder = ACC_Utilities.GetToolRootPath(this);
            TryGetDefaultMaterial();

            for (int i = 0; i < cableSequences.Count; i++)
            {
                if (cableSequences[i])
                {
                    cableSequences[i].OnAnyCableRemoved -= RemoveTrailSegment;
                    cableSequences[i].OnAnyCableRemoved += RemoveTrailSegment;
                }
            }
        }
        private void OnDisable()
        {
            for (int i = 0; i < cableSequences.Count; i++)
            {
                if (cableSequences[i])
                {
                    cableSequences[i].OnAnyCableRemoved -= RemoveTrailSegment;
                }
            }
        }

        private void RemoveTrailSegment(int cableID, ACC_Cable cable)
        {
            controlPointsStart.RemoveAt(cableID);
            controlPointsEnd.RemoveAt(cableID);
            prevPosStart = new Vector3[controlPointsStart.Count];
            prevPosEnd = new Vector3[controlPointsEnd.Count];

            for (int i = 0; i < cableSequences.Count; i++)
            {
                for (int j = 0; j < cableSequences[i].cables.Count; j++)
                {
                    if (j == cableID)
                    {
                        if (cableSequences[i].cables[j] != null)
                        {
                            ACC_Cable _cable = cableSequences[i].cables[j];

                            if (cable != _cable)
                            {
                                cableSequences[i].Unsubscribe(j);
                                cableSequences[i].cables.Remove(_cable);
                                DestroyImmediate(_cable.gameObject);
                            }
                        }

                        break;
                    }
                }
            }
            Selection.activeGameObject = gameObject;
        }

        public void UpdatePrevPoints()
        {
            for (int i = 0; i < controlPointsStart.Count; i++)
            {
                prevPosStart[i] = controlPointsStart[i];
                prevPosEnd[i] = controlPointsEnd[i];
            }
        }

        public void SetControlPoints()
        {
            if (controlPoints.Count >= 2)
            {
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (controlPoints[i] == null) return;
                }
                ACC_Cable[] cables = GetComponentsInChildren<ACC_Cable>();
                for (int i = 0; i < cables.Length; i++)
                {
                    if (!cables[i]) continue;
                    if (cables[i].transform.parent.parent == transform) DestroyImmediate(cables[i].gameObject);
                }

                controlPointsStart = new List<Vector3>();
                controlPointsEnd = new List<Vector3>();

                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (i > 0)
                    {
                        controlPointsEnd.Add(controlPoints[i].position);
                    }
                    if (i < controlPoints.Count - 1)
                    {
                        controlPointsStart.Add(controlPoints[i].position);
                    }
                }

                for (int i = 0; i < controlPointsStart.Count; i++)
                {
                    AddSegment();
                }
            }
            else
            {
                prevPosStart = new Vector3[controlPointsStart.Count];
                prevPosEnd = new Vector3[controlPointsEnd.Count];
            }
        }

        void TryGetDefaultMaterial()
        {
            string mtlPath = toolRootFolder + "/Materials/CableC.mat";
           
            if(materials.Length == 0 || materials[0] == null)
            {
                if(materials.Length == 0) materials = new Material[1];

                materials[0] = AssetDatabase.LoadAssetAtPath(mtlPath, typeof(Material)) as Material;
            }
        }

        public void CreateFirstCableSegment(Transform trailRoot)
        {
            Vector3 posStart = Vector3.zero;
            Vector3 posEnd = Vector3.zero;

            posStart = Vector3.forward * -0.5f;
            posEnd = Vector3.forward * 0.5f;

            //add control points here
            controlPointsStart.Add(posStart);
            controlPointsEnd.Add(posEnd);

            prevPosStart = new Vector3[controlPointsStart.Count];
            prevPosEnd = new Vector3[controlPointsEnd.Count];

            for (int i = 0; i < amount; i++)
            {
                ACC_Sequence newCableSequence = new GameObject("Cable Sequence", typeof(ACC_Sequence)).GetComponent<ACC_Sequence>();

                newCableSequence.meshSettingsOverride = false;
                newCableSequence.shapeSettingsOverride = false;
                newCableSequence.jointSettingsOverride = false;

                newCableSequence.transform.parent = trailRoot;
                newCableSequence.transform.localPosition = Vector3.zero;

                newCableSequence.CreateNewCable(newCableSequence.transform, radialSegments, lengthSegments, true);

                newCableSequence.cables[0].cableStart = posStart;
                newCableSequence.cables[0].cableEnd = posEnd;

                cableSequences.Add(newCableSequence);

                newCableSequence.OnAnyCableRemoved += RemoveTrailSegment;
            }
        }

        public void ChangeAmount(int increment)
        {
            amount += increment;

            if (increment > 0)
            {
                ACC_Sequence newCableSequence = new GameObject("Cable Sequence", typeof(ACC_Sequence)).GetComponent<ACC_Sequence>();

                newCableSequence.meshSettingsOverride = false;
                newCableSequence.shapeSettingsOverride = false;
                newCableSequence.jointSettingsOverride = false;

                newCableSequence.transform.parent = transform;
                newCableSequence.transform.localPosition = Vector3.zero;

                newCableSequence.CreateNewCable(newCableSequence.transform, radialSegments, lengthSegments, true);

                newCableSequence.cables[0].cableStart = transform.position;
                newCableSequence.cables[0].cableEnd = transform.position + Vector3.forward;

                cableSequences.Add(newCableSequence);

                newCableSequence.OnAnyCableRemoved += RemoveTrailSegment; 

                for (int i = 0; i < cableSequences[0].cables.Count - 1; i++)
                {
                    newCableSequence.CreateNewCable(newCableSequence.transform, radialSegments, lengthSegments, false);
                }

                SwitchShowMeshes();
                UpdateSequencesPositions();

                UpdateUVs();
                UpdateHeightScale();
                UpdateAllSequences();
            }
            else
            {
                cableSequences[cableSequences.Count - 1].OnAnyCableRemoved -= RemoveTrailSegment;
                DestroyImmediate((cableSequences[cableSequences.Count - 1].gameObject));
                cableSequences.RemoveAt(cableSequences.Count - 1);
            }
            UpdateSequencesPositions();
        }

        public void UpdateUVs()
        {
            for (int i = 0; i < cableSequences.Count; i++)
            {
                float randomOffset = Random.Range(0, textureOffsetRandomness * 0.5f);

                cableSequences[i].UpdateUVs(randomOffset);
            }
        }

        public void UpdateHeightScale()
        {
            for (int i = 0; i < cableSequences.Count; i++)
            {
                cableSequences[i].heightScale = heightScale;
                //cableSequences[i].UpdateAllCables(false);
            }
        }

        public void UpdateAllSequences()
        {
            for (int i = 0; i < cableSequences.Count; i++)
            {
                cableSequences[i].pointsOffset = pointsOffset;

                if (!cableSequences[i].meshSettingsOverride)
                {
                    cableSequences[i].radialSegments = radialSegments;
                    cableSequences[i].angle = angle;
                    cableSequences[i].lengthSegments = lengthSegments;
                    cableSequences[i].invert = invert;
                    cableSequences[i].vertexAlphaBrightness = vertexAlphaBrightness;
                    cableSequences[i].textureTilingMultiplier = textureTilingMultiplier;
                    cableSequences[i].uvsAngle = uvsAngle;
                    cableSequences[i].uvTwist = Random.Range(uvTwist.x, uvTwist.y);

                    if (materials.Length > 0)
                    {
                        Material mtl = materials[Random.Range(0, materials.Length)];
                        cableSequences[i].material = mtl;
                    }
                }

                if (!cableSequences[i].shapeSettingsOverride)
                {
                    cableSequences[i].thickness = Random.Range(thickness.x, thickness.y);
                    cableSequences[i].lengthDependentHeight = lengthDependentHeight;
                    cableSequences[i].horizontalCurvature = horizontalCurvature;
                    cableSequences[i].verticalCurvature = verticalCurvature;
                    cableSequences[i].noise = noise;
                }

                if (!cableSequences[i].jointSettingsOverride)
                {
                    cableSequences[i].jointObj = jointObj;
                    cableSequences[i].jointRotation = jointRotation;
                    cableSequences[i].jointObjScale = jointObjScale;
                }

                cableSequences[i].UpdateAllCables(false);
            }
        }

        public void UpdateJointObjects()
        {
            for (int i = 0; i < cableSequences.Count; i++)
            {
                for (int j = 0; j < cableSequences[i].cables.Count; j++)
                {
                    cableSequences[i].cables[j].SetJointObject();
                }
            }
        }

        public void SwitchShowMeshes()
        {
            for (int i = 0; i < cableSequences.Count; i++)
            {
                for (int j = 0; j < cableSequences[i].cables.Count; j++)
                {
                    cableSequences[i].cables[j].showMeshes = showMeshes;
                    cableSequences[i].cables[j].meshRenderer.enabled = showMeshes;
                    cableSequences[i].cables[j].sequenceGizmos = !showMeshes;
                }
            }
        }

        public void AddSegment()
        {
            //add control points here
            if (controlPoints.Count < 2)
            {
                Vector3 posStart = Vector3.zero;
                Vector3 posEnd = Vector3.zero;

                posStart = controlPointsEnd[controlPointsEnd.Count - 1];
                posEnd = controlPointsEnd[controlPointsEnd.Count - 1] + (controlPointsEnd[controlPointsEnd.Count - 1] - controlPointsStart[controlPointsStart.Count - 1]).normalized;

                controlPointsStart.Add(posStart);
                controlPointsEnd.Add(posEnd);

                prevPosStart = new Vector3[controlPointsStart.Count];
                prevPosEnd = new Vector3[controlPointsEnd.Count];
            }

            for (int i = 0; i < cableSequences.Count; i++)
            {
                cableSequences[i].CreateNewCable(cableSequences[i].transform, radialSegments, lengthSegments, controlPoints.Count >= 2);
            }
        }

        public void CalculateRandomSteps()
        {
            // calculate random steps
            stepX_sums = new float[controlPointsStart.Count];
            stepY_sums = new float[controlPointsStart.Count];
            stepZ_sums = new float[controlPointsStart.Count];
            steps = new Vector3[controlPointsStart.Count];

            stepsX = new float[controlPointsStart.Count][];
            stepsY = new float[controlPointsStart.Count][];
            stepsZ = new float[controlPointsStart.Count][];
            for (int i = 0; i < stepsX.Length; i++)
            {
                stepsX[i] = new float[cableSequences.Count];
                stepsY[i] = new float[cableSequences.Count];
                stepsZ[i] = new float[cableSequences.Count];
            }

            for (int i = 0; i < controlPointsStart.Count; i++)
            {
                for (int j = 0; j < stepsX[i].Length; j++)
                {
                    float maxThickness = Mathf.Max(thickness.x, thickness.y);
                    if (positionRandomness.x > 0)
                    {
                        stepsX[i][j] = Mathf.Lerp(maxThickness, Random.Range(maxThickness, stepSize.x), positionRandomness.x);
                        stepX_sums[i] += stepsX[i][j];
                    }
                    else
                    {
                        stepsX[i][j] = stepSize.x;
                        stepX_sums[i] += stepSize.x;
                    }

                    if (positionRandomness.y > 0)
                    {
                        if (stepSize.x > 0)
                        {
                            stepsY[i][j] = Mathf.Lerp(0, Random.Range(-stepSize.y, stepSize.y), positionRandomness.y);
                        }
                        else
                        {
                            stepsY[i][j] = Mathf.Lerp(maxThickness, Random.Range(maxThickness, stepSize.y), positionRandomness.y);
                        }
                        stepY_sums[i] += stepsY[i][j];
                    }
                    else
                    {
                        stepsY[i][j] = stepSize.y;
                        stepY_sums[i] += stepSize.y;
                    }

                    if (positionRandomness.z > 0)
                    {
                        stepsZ[i][j] = Mathf.Lerp(0, Random.Range(-stepSize.z, stepSize.z), positionRandomness.z);
                        stepZ_sums[i] += stepsZ[i][j];
                    }
                    else
                    {
                        stepsZ[i][j] = stepSize.z;
                        stepZ_sums[i] += stepSize.z;
                    }
                }
                stepX_sums[i] += stepX_sums[i] / stepsX[i].Length;
                stepX_sums[i] = -stepX_sums[i] * 0.5f;

                stepY_sums[i] += stepY_sums[i] / stepsY[i].Length;
                stepY_sums[i] = -stepY_sums[i] * 0.5f;

                stepZ_sums[i] += stepZ_sums[i] / stepsZ[i].Length;
                stepZ_sums[i] = -stepZ_sums[i] * 0.5f;
            }
        }

        public void UpdateSequencesPositions()
        {
            CalculateRandomSteps();

            Vector3 pointPosStart;
            Vector3 pointPosEnd;
            Vector3 pointPosStartPrev;

            float lengthFraction = 0;
            float maxLength = 0;

            //apply positions
            for (int s = 0; s < cableSequences.Count; s++)
            {
                Vector3 posStart = Vector3.zero;
                Vector3 posEnd = Vector3.zero;

                for (int c = 0; c < cableSequences[s].cables.Count; c++)
                {
                    ACC_Cable cable = cableSequences[s].cables[c];

                    int subtractID = c == 0 ? 0 : 1;

                    //lock start control point to the end control point for every cable segment
                    controlPointsStart[c] = c >= 1 ? controlPointsEnd[c - 1] : controlPointsStart[c];

                    //get horizontal and verical perpendiculars             
                    if (controlPoints.Count < 2)
                    {
                        pointPosStart = transform.TransformPoint(controlPointsStart[c]);
                        pointPosEnd = transform.TransformPoint(controlPointsEnd[c]);
                        pointPosStartPrev = transform.TransformPoint(controlPointsStart[c - subtractID]);
                    }

                    else
                    {
                        pointPosStart = controlPointsStart[c];
                        pointPosEnd = controlPointsEnd[c];
                        pointPosStartPrev = controlPointsStart[c - subtractID];
                    }

                    sectionNormalH = SectionNormal(pointPosStartPrev, pointPosStart, pointPosEnd, Vector3.up);
                    sectionNormalV = Vector3.Cross(sectionNormalH, (pointPosStartPrev - pointPosEnd).normalized);

                    //get forward direction
                    Vector3 dirZ = Quaternion.AngleAxis(90, Vector3.up) * sectionNormalH;

                    //increment XY steps
                    steps[c].x += stepsX[c][s];
                    steps[c].y += stepsY[c][s];

                    //increment Z steps for all points except 0 point
                    if (c != 0)
                    {
                        steps[c].z += stepsZ[c][s];
                    }
                    else
                    {
                        steps[c].z = 0;
                    }

                    //calculate final position and offset based on the perpendicular vectors and steps
                    Vector3 finalPos = (sectionNormalH * steps[c].x) + (sectionNormalV * steps[c].y) + (dirZ * steps[c].z);
                    Vector3 finalOffset = (sectionNormalH * stepX_sums[c]) + (sectionNormalV * stepY_sums[c]) + (dirZ * stepZ_sums[c]);

                    //set positions
                    posStart = pointPosStart + finalPos + finalOffset;
                    posEnd = pointPosEnd + finalPos + finalOffset;

                    //recalculate and set position for the last point
                    if (c == cableSequences[s].cables.Count - 1)
                    {
                        sectionNormalH = SectionNormal(pointPosStart, pointPosStart, pointPosEnd, Vector3.up);
                        sectionNormalV = Vector3.Cross(sectionNormalH, (pointPosStart - pointPosEnd).normalized);

                        posEnd = pointPosEnd + (sectionNormalH * steps[0].x) + (sectionNormalV * steps[0].y) + (sectionNormalH * stepX_sums[0]) + (sectionNormalV * stepY_sums[0]);
                    }

                    //apply positions
                    cableSequences[s].cables[c].cableStart = posStart;
                    cableSequences[s].cables[c].cableEnd = posEnd;

                    //get the longest cable length and calculate the lengthFraction
                    if(s == 0)
                    {
                        float cableLength = Vector3.Distance(cableSequences[0].cables[c].cableStart, cableSequences[0].cables[c].cableEnd);
                        maxLength = Mathf.Max(maxLength, cableLength);
                        lengthFraction = 1 / maxLength;
                    }

                    cableSequences[s].cables[c].lengthFraction = lengthFraction;

                    cable.CatchControlPointMove(true);
                }
            }
        }

        Vector3 SectionNormal(Vector3 start, Vector3 mid, Vector3 end, Vector3 axis)
        {
            Vector3 v = (end - start);

            v = Vector3.ProjectOnPlane(v.normalized, axis).normalized;

            Vector3 averagePoint = (start + end) / 2;

            Vector3 dirToAverage = (averagePoint - mid);
            dirToAverage = Vector3.ProjectOnPlane(dirToAverage.normalized, axis).normalized;

            Vector3 cross = Vector3.Cross(v, mid - start);
            if (cross.y > 0)
            {
                dirToAverage = -dirToAverage;
            }

            Vector3 perpendicular = Quaternion.AngleAxis(90, axis) * v.normalized;

            float t = 0;
            if (start == mid)
            {
                t = 1;
            }
            else
            {
                float dot = Vector3.Dot((start - mid).normalized, (end - mid).normalized);
                t = 0.5f + Mathf.Clamp(-dot, 0, 1);
            }
            Vector3 n = Vector3.Lerp(dirToAverage, perpendicular, t);
            if (stepSize.x == 0)
            {
                n = perpendicular;
            }

            return n.normalized;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "NOT_Lonely/IconTrail.png", true);
        }
        
#endif
    }
}

