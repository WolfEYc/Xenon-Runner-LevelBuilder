
namespace NOT_Lonely
{
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]

    [ExecuteInEditMode]
    public class ACC_Cable : MonoBehaviour
    {
        //[Header("CONTROL POINTS (OPTIONAL)")]
        public Vector3 pointsOffset = Vector3.zero;
        public Transform startPoint;
        public Transform endPoint;
        [HideInInspector] public Vector3 cableStart;
        [HideInInspector] public Vector3 cableEnd;
        [HideInInspector] public Vector3 cornerPos;
        [HideInInspector] public Vector3 cornerPosCurveStart;
        [HideInInspector] public Vector3 cornerPosCurveEnd;

        //[Header("MESH")]
        public int radialSegments = 2;
        public float angle = 0;
        public int lengthSegments = 4;
        public bool invert = false;
        public float vertexAlphaBrightness = 0;
        public float textureTilingMultiplier = 1;
        [HideInInspector] public bool horizontalUVs;
        [HideInInspector] public bool isSquareTex;
        public int uvsAngle = 0;
        public float uvTwist = 0;
        [HideInInspector] public float uvTwistOffset;

        public Material material;

        //[Header("SHAPE")]
        public float heightScale = 0.1f;
        public float lengthDependentHeight = 0.5f;
        public float thickness = 0.05f;
        public bool useCurvature = true;
        public float noise = 0;

        //[Header("JOINT")]
        public Transform jointObj;
        public Vector3 jointRotation = Vector3.zero;
        public Vector3 jointObjScale = Vector3.one;
        public bool uniformScale = true;
        public float scale = 1;

        //[Header("GIZMOS")]
        [HideInInspector] public bool sequenceGizmos = false;
        [HideInInspector] public bool showMeshes = true;

        private struct Vertices
        {
            public Vector3[] point;
            public float[] radius;
            public Color[] color;
        }

        private float tilingModifier;

        [HideInInspector] public ACC_CableJoint _jointObj;

        private int crossSegments;
        private Vertices vertices;

        private MeshFilter filter;
        public MeshRenderer meshRenderer;
        private Mesh mesh;

        private Vector3[] crossPoints;
        private int lastCrossSegments;

        public float length = 1;
        public float lengthFraction = 1;

        Vector3 startCurvePosEnd;
        Vector3 endCurvePosStart;

        [HideInInspector] public float startCurveSideModifier;
        [HideInInspector] public float endCurveSideModifier;
        [HideInInspector] public float verticalCurvature = 1;
        [HideInInspector] public float horizontalCurvature = 1;

        [HideInInspector] public Vector3 lastPosStart = Vector3.zero;
        [HideInInspector] public Vector3 lastPosEnd = Vector3.zero;
        [HideInInspector] public Vector3 lastObjectPos = Vector3.zero;

        private int prevSegs = 0;
        private int prevRadialSegs = 0;
        private bool prevCurvatureState = true;
        [HideInInspector] public float uvOffset = 0;

        private int vcount;
        public Vector3[] meshVertices;
        public List<Vector3> _meshVertices = new List<Vector3>();
        private Vector2[] uvs;
        private Color[] colors;
        public int[] tris;
        private List<int> _tris = new List<int>();

        private Shader shader;
        public string toolRootFolder;

        public bool meshSettingsOverride = true;
        public bool shapeSettingsOverride = true;
        public bool jointSettingsOverride = true;

        public delegate void CableStartMoved(ACC_Cable cable);
        public delegate void CableEndMoved(ACC_Cable cable);
        public event CableStartMoved OnCableStartMoved;
        public event CableEndMoved OnCableEndMoved;

        public delegate void CableRemoved(ACC_Cable cable);
        public event CableRemoved OnCableRemoved;
        public float hStep;

#if UNITY_EDITOR
        
        void OnEnable()
        {
            if(Application.isPlaying) return;
            
            toolRootFolder = ACC_Utilities.GetToolRootPath(this);
            TryGetDefaultMaterial();

            filter = gameObject.GetComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.name = "ProceduralCable";
            filter.sharedMesh = mesh;

            meshRenderer = gameObject.GetComponent<MeshRenderer>();

            filter.hideFlags = HideFlags.HideInInspector;
            meshRenderer.hideFlags = HideFlags.HideInInspector;

            FindPipelineShader();
            if (!_jointObj) SetJointObject();

            SetMaterial();

            EditorApplication.update += OnEditorUpdate;

            ReDraw();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnDestroy()
        {
            OnCableRemoved?.Invoke(this);
        }

        public void ResetPivot()
        {
            transform.position = (_cableEnd + _cableStart) / 2;
        }

        void FindPipelineShader()
        {
            if (Shader.Find("Universal Render Pipeline/Lit"))
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            else if (Shader.Find("HDRP/Lit"))
            {
                shader = Shader.Find("HDRP/Lit");
            }
            else
            {
                shader = Shader.Find("Standard");
            }
        }

        void TryGetDefaultMaterial()
        {
            string mtlPath = toolRootFolder + "/Materials/CableC.mat";
            if (!material) material = AssetDatabase.LoadAssetAtPath(mtlPath, typeof(Material)) as Material;
        }

        float ratio;
        public void SetMaterial()
        {
            if (!meshRenderer) meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (material)
            {
                meshRenderer.sharedMaterial = material;
                Texture textureMap = ACC_Utilities.GetTex(material);

                if (textureMap != null)
                {
                    horizontalUVs = textureMap.height < textureMap.width;
                    isSquareTex = textureMap.height == textureMap.width;

                    if (!isSquareTex)
                    {
                        if (!horizontalUVs)
                        {
                            ratio = ((float)(textureMap.height / textureMap.width) / 2);
                            tilingModifier = 1 / ratio;
                        }
                        else
                        {
                            ratio = ((float)(textureMap.width / textureMap.height) / 2);
                            tilingModifier = 1 / ratio;
                        }
                        if (tilingModifier == 1)
                        {
                            ratio = 1;
                            tilingModifier = 0.5f;
                        }
                    }
                    else
                    {
                        ratio = 1;
                        tilingModifier = 1;
                    }
                }
                else
                {
                    tilingModifier = 1;
                }
                if (tilingModifier == 0.25f) ratio = 2;
            }
            else
            {
                if (!meshRenderer.sharedMaterial)
                {
                    meshRenderer.sharedMaterial = new Material(shader);
                    meshRenderer.sharedMaterial.color = Color.gray;
                }
            }
        }

        public void SetJointObject()
        {
            if (_jointObj) DestroyImmediate(_jointObj.gameObject);

            if (jointObj)
            {
                var obj = PrefabUtility.InstantiatePrefab(jointObj, transform);
                Transform objTransform = obj as Transform;
                _jointObj = objTransform.gameObject.AddComponent<ACC_CableJoint>();

                RecalculateJointObject();
            }
        }

        void OnDrawGizmos()
        {
            if (!sequenceGizmos)
                return;

            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_cableStart, thickness / 2);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_cableEnd, thickness / 2);

            Vector3[] gizmosSequence = CreateLinePointsSequence();

            for (int i = 0; i < gizmosSequence.Length - 1; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(gizmosSequence[i], gizmosSequence[i + 1]);
            }

            Gizmos.DrawWireSphere(startCurvePosEnd, thickness / 2);
            Gizmos.DrawWireSphere(endCurvePosStart, thickness / 2);
        }

        public void CatchControlPointMove(bool isStartControlPoint)
        {
            if (isStartControlPoint)
            {
                OnCableStartMoved?.Invoke(this);
            }
            else
            {
                OnCableEndMoved?.Invoke(this);
            }
        }

        void Draw(Vector3[] positions)
        {
            vertices.point = new Vector3[positions.Length];
            vertices.radius = new float[positions.Length];
            vertices.color = new Color[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                vertices.point[i] = positions[i] - this.transform.position;

                vertices.radius[i] = thickness / 2;
                vertices.color[i] = Color.white;
            }

            if (radialSegments < 2) radialSegments = 2;

            crossSegments = radialSegments == 2 ? 2 : radialSegments + 1;
            vcount = vertices.point.Length;
            meshVertices = new Vector3[vcount * crossSegments];
            uvs = new Vector2[vcount * crossSegments];
            colors = new Color[vcount * crossSegments];

            tris = new int[vcount * crossSegments * 6];

            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] = -1;
            }

            if (showMeshes) Rebuild();
        }

        Vector3 _cableStart;
        Vector3 _cableEnd;

        public void ReDraw()
        {
            _cableStart = cableStart + pointsOffset;
            _cableEnd = cableEnd + pointsOffset;

            Draw(CreateLinePointsSequence());
            RecalculateJointObject();
        }

        public void RecalculateJointObject()
        {
            if (!_jointObj) return;

            _jointObj.transform.localScale = jointObjScale * thickness;
            _jointObj.transform.position = _cableStart;
        }

        Quaternion _rotation;

        public void UpdateJointObjectRotation(Quaternion rotation)
        {
            if (!_jointObj) return;
            if (!_jointObj.autoUpdateRotation) return;

            _rotation = rotation;
            _jointObj.transform.rotation = _rotation;
            _jointObj.transform.Rotate(jointRotation, Space.Self);
        }

        public void UpdateJointObjectRotation()
        {
            if (!_jointObj) return;
            if (!_jointObj.autoUpdateRotation) return;

            _jointObj.transform.rotation = _rotation;
            _jointObj.transform.Rotate(jointRotation, Space.Self);
        }

        int startCurvePosCount = 0;
        int endCurvePosCount = 0;
        Vector3[] CreateLinePointsSequence()
        {
            // Make sure lengthSegments are never below 4
            if (lengthSegments < 4) lengthSegments = 4;

            thickness = Math.Max(0.001f, thickness);

            //Make sure lengthSegments are even
            int linePositionsNum = lengthSegments * 2 + 1;

            //Height difference between points
            float deltaHeight = _cableStart.y - _cableEnd.y;

            //Vector between points
            Vector3 lineVector = _cableEnd - _cableStart;

            length = Vector3.Distance(cableEnd, cableStart);

            //Create an array of positions
            Vector3[] linePositions = new Vector3[linePositionsNum];

            float lengthDependency = 1;
            if (lengthDependentHeight > 0)
            {
                lengthDependency = lengthDependentHeight * lineVector.sqrMagnitude;
            }

            cornerPos = (_cableStart + _cableEnd) / 2;
            cornerPos.y += -heightScale * lengthDependency;

            float startCornerDeltaHeight = _cableStart.y - cornerPos.y;
            float endCornerDeltaHeight = _cableEnd.y - cornerPos.y;

            float heightMultiplier = 0.05f;

            Vector3 cableNormal = (Quaternion.AngleAxis(90, Vector3.up) * (_cableEnd - _cableStart)).normalized;
            cableNormal = Vector3.ProjectOnPlane(cableNormal, Vector3.up).normalized;

            startCurvePosEnd = Vector3.Lerp(_cableStart, cornerPos, 0.1f) + (Vector3.up * heightMultiplier * startCornerDeltaHeight * verticalCurvature) + (cableNormal * startCurveSideModifier * horizontalCurvature * thickness * 0.02f) / thickness;
            endCurvePosStart = Vector3.Lerp(_cableEnd, cornerPos, 0.1f) + (Vector3.up * heightMultiplier * endCornerDeltaHeight * verticalCurvature) + (cableNormal * endCurveSideModifier * horizontalCurvature * thickness * 0.02f) / thickness;

            Vector3 startCurveNormal = (Quaternion.AngleAxis(90, Vector3.up) * (startCurvePosEnd - _cableStart)).normalized;
            startCurveNormal = Vector3.ProjectOnPlane(startCurveNormal, Vector3.up);
            cornerPosCurveStart = ((_cableStart + startCurvePosEnd) / 2) + (startCurveNormal * startCurveSideModifier * horizontalCurvature * thickness * 0.018f) / thickness;
            cornerPosCurveStart.y += 0.03f * startCornerDeltaHeight * verticalCurvature;

            Vector3 endCurveNormal = (Quaternion.AngleAxis(90, Vector3.up) * (_cableEnd - endCurvePosStart)).normalized;
            endCurveNormal = Vector3.ProjectOnPlane(endCurveNormal, Vector3.up);
            cornerPosCurveEnd = ((_cableEnd + endCurvePosStart) / 2) + (endCurveNormal * endCurveSideModifier * horizontalCurvature * thickness * 0.018f) / thickness;
            cornerPosCurveEnd.y += 0.03f * endCornerDeltaHeight * verticalCurvature;

            startCurvePosCount = (linePositionsNum - 1) / 4;
            endCurvePosCount = (linePositionsNum - 1) / 4;

            if (!useCurvature)
            {
                for (int i = 0; i < linePositionsNum; i++)
                {
                    float t = i / ((float)linePositionsNum - 1);

                    linePositions[i] = QuadraticBezierCurve(t, _cableStart, cornerPos, _cableEnd);
                }
            }
            else
            {
                //start curve
                for (int i = 0; i < startCurvePosCount; i++)
                {
                    float t = i * (1 / ((float)startCurvePosCount - 1));

                    linePositions[i] = QuadraticBezierCurve(t, _cableStart, cornerPosCurveStart, startCurvePosEnd);
                }

                //mid curve (main)
                for (int i = startCurvePosCount; i < linePositionsNum - endCurvePosCount; i++)
                {
                    float t = (i - startCurvePosCount) / ((float)linePositionsNum - startCurvePosCount - endCurvePosCount - 1);

                    linePositions[i] = QuadraticBezierCurve(t, startCurvePosEnd, cornerPos, endCurvePosStart);
                }

                //end curve
                for (int i = linePositionsNum - endCurvePosCount; i < linePositionsNum; i++)
                {
                    float t = (i - (linePositionsNum - endCurvePosCount)) / ((float)endCurvePosCount - 1);
                    linePositions[i] = QuadraticBezierCurve(t, endCurvePosStart, cornerPosCurveEnd, _cableEnd);
                }
            }

            List<Vector3> _linePositions = new List<Vector3>();

            for (int i = 0; i < linePositions.Length; i++)
            {
                if (!_linePositions.Contains(linePositions[i]))
                {
                    _linePositions.Add(linePositions[i]);
                }
            }

            return _linePositions.ToArray();
        }

        public Vector3 QuadraticBezierCurve(float t, Vector3 startPos, Vector3 midPos, Vector3 endPos)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            Vector3 p = uu * startPos;
            p += 2 * u * t * midPos;
            p += tt * endPos;
            return p;
        }

        public Vector3 CubicBezierCurve(float t, Vector3 startPos, Vector3 cornerPosA, Vector3 cornnerPosB, Vector3 endPos)
        {
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * startPos +
                3f * oneMinusT * oneMinusT * t * cornnerPosB +
                3f * oneMinusT * t * t * cornerPosA +
                t * t * t * endPos;
        }

        Vector3 noiseVector;
        
        public void Rebuild()
        {
            float multiplier = radialSegments == 2 ? 2 : 1;

            if (lengthSegments < 1) lengthSegments = 1;

            if (crossSegments != lastCrossSegments)
            {
                crossPoints = new Vector3[crossSegments];
                float theta = 2.0f * Mathf.PI / (radialSegments == 2 ? 2 : crossSegments - 1);
                for (int c = 0; c < crossSegments; c++)
                {
                    crossPoints[c] = new Vector3(Mathf.Cos(theta * c), Mathf.Sin(theta * c), 0);
                }
                lastCrossSegments = crossSegments;
            }

            int[] lastVertices = new int[crossSegments];
            int[] theseVertices = new int[crossSegments];
            Quaternion rotation = Quaternion.identity;

            float uvStepH = 1.0f / (radialSegments == 2 ? 2 : crossSegments - 1);
            float uvStepV = 1.0f / (lengthSegments / 2);

            Vector3[] linePointsSequence = CreateLinePointsSequence();

            float v = 0;
            float u = 0;
            float vStep = 0;
            hStep = 0;
            Vector3 prevPoint = linePointsSequence[0];
            float pointsDistanceSum = 0;
            float vertexColor = 0;
            float divider = (float)(vcount - 1);
            float caclulatedVertexValue;
            noiseVector = Vector3.zero;

            //random R channel value for the amplitude randomization in the shader 
            float r = UnityEngine.Random.Range(0f, 1f);

            for (int p = 0; p < vcount; p++)
            {
                if (p < vcount - 1)
                {
                    if (vertices.point[p + 1] - vertices.point[p] != Vector3.zero)
                    {
                        rotation = Quaternion.LookRotation(vertices.point[p + 1] - vertices.point[p], Vector3.up);
                        rotation = Quaternion.AngleAxis(Mathf.Lerp(0, 360, angle), (vertices.point[p + 1] - vertices.point[p]).normalized) * rotation;
                    }
                }

                //create a vertex color gradient
                if (p < divider / 2)
                {
                    //calculate a start to center gradient
                    caclulatedVertexValue = (p / (divider - (divider / 2)));

                    //calculate a length dependent vertex color
                    vertexColor = caclulatedVertexValue * length * lengthFraction * Mathf.Clamp01(vertexAlphaBrightness);
                }
                else
                {
                    //calculate a center to end gradient
                    caclulatedVertexValue = (1 - ((p - (divider - (divider / 2))) / (divider - (divider / 2))));

                    //calculate a length dependent vertex color
                    vertexColor = caclulatedVertexValue * length * lengthFraction * Mathf.Clamp01(vertexAlphaBrightness);
                }

                //get random noise vector
                noiseVector = new Vector3(UnityEngine.Random.Range(0f, caclulatedVertexValue), UnityEngine.Random.Range(0f, caclulatedVertexValue), UnityEngine.Random.Range(0f, caclulatedVertexValue)) * 0.1f * noise;
                if (p >= vcount - 3) noiseVector = Vector3.zero;

                pointsDistanceSum += Vector3.Distance(prevPoint, linePointsSequence[p]);

                hStep = pointsDistanceSum * uvTwist + uvTwistOffset;

                for (int c = 0; c < crossSegments; c++)
                {
                    int vertexIndex = p * crossSegments + c;
                    meshVertices[vertexIndex] = vertices.point[p] + rotation * crossPoints[c] * vertices.radius[p] + noiseVector;

                    //UVs
                    if (horizontalUVs)
                    {
                        v = c * uvStepH + hStep;

                        uvs[vertexIndex] = new Vector2(u * multiplier + uvOffset, v * multiplier);
                    }
                    else
                    {
                        u = c * -uvStepH + 1 + hStep;
                        uvs[vertexIndex] = new Vector2(u * multiplier, v * multiplier + uvOffset);
                    }

                    lastVertices[c] = theseVertices[c];
                    theseVertices[c] = p * crossSegments + c;

                    colors[vertexIndex] = vertices.color[p];

                    colors[vertexIndex] = new Color(r, 0, 0, vertexColor);
                }

                prevPoint = linePointsSequence[p];

                if (p < vcount - 1)
                {
                    vStep = Vector3.Distance(linePointsSequence[p], linePointsSequence[p + 1]);
                }
                else
                {
                    if (p > 1)
                    {
                        vStep = Vector3.Distance(linePointsSequence[p], linePointsSequence[p - 1]);
                    }
                }

                float extraMultiplier = radialSegments == 2 ? 1.5f : 1;

                if (horizontalUVs)
                {
                    u += (vStep / thickness) * (tilingModifier / (ratio * 3)) * textureTilingMultiplier * extraMultiplier;
                }
                else
                {
                    v += (vStep / thickness) * (tilingModifier / (ratio * 3)) * textureTilingMultiplier * extraMultiplier;
                }

                for (int c = 0; c < crossSegments; c++)
                {
                    if (p == 0 || c >= crossPoints.Length - 1)
                    {
                        continue;
                    }

                    int start = (p * crossSegments + c) * 6;

                    tris[start] = lastVertices[c];
                    tris[start + 1] = lastVertices[(c + 1) % crossSegments];
                    tris[start + 2] = theseVertices[c];

                    tris[start + 3] = tris[start + 2];
                    tris[start + 4] = tris[start + 1];
                    tris[start + 5] = theseVertices[(c + 1) % crossSegments];
                }
            }

            //double tris count strange workaround :D
            #region Double tris count strange fix :D
            _tris.Clear();

            for (int i = 0; i < tris.Length; i++)
            {
                if (tris[i] != -1) _tris.Add(tris[i]);
            }

            tris = new int[_tris.Count];

            if (invert) _tris.Reverse();

            tris = _tris.ToArray();

            #endregion

            if (prevRadialSegs > radialSegments || prevSegs > lengthSegments)
            {
                mesh.triangles = tris;
                mesh.vertices = meshVertices;
            }
            else
            {
                mesh.vertices = meshVertices;
                mesh.triangles = tris;
            }

            mesh.colors = colors;

            mesh.RecalculateNormals();

            //average normals on the cable wrap vertices
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < meshVertices.Length; i++)
            {
                for (int j = i + 1; j < meshVertices.Length; j++)
                {
                    if (meshVertices[i] == meshVertices[j])
                    {
                        Vector3 averagedNormal = (normals[i] + normals[j]) / 2;
                        normals[i] = averagedNormal;
                        normals[j] = averagedNormal;
                    }
                }
            }

            mesh.normals = normals;

            mesh.RecalculateBounds();

            //rotate the generated UVs
            if (isSquareTex)
            {
                float uvRot = uvsAngle * Mathf.PI / 180;

                float sin = Mathf.Sin(uvRot);
                float cos = Mathf.Cos(uvRot);

                for (int i = 0; i < uvs.Length; i++)
                {
                    float x = uvs[i].x * cos + uvs[i].y * sin;
                    float y = uvs[i].x * -sin + uvs[i].y * cos;

                    uvs[i].x = x;
                    uvs[i].y = y;
                }
            }
            else
            {
                uvsAngle = 0;
            }

            mesh.uv = uvs;
            mesh.RecalculateTangents();

            prevSegs = lengthSegments;
            prevRadialSegs = radialSegments;
            prevCurvatureState = useCurvature;
        }

        Vector3 prevPos;
        void OnEditorUpdate()
        {
            if (Application.isPlaying) return;
            if (!Selection.Contains(gameObject)) return;

            if (meshSettingsOverride || shapeSettingsOverride || jointSettingsOverride)
            {
                if (gameObject.name != "Cable (Overrides are used)")
                {
                    gameObject.name = "Cable (Overrides are used)";
                }
            }
            else
            {
                if (gameObject.name != "Cable")
                {
                    gameObject.name = "Cable";
                }
            }

            ResetPivot();
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (transform.position != prevPos) ReDraw();

            prevPos = transform.position;
        }
        
#endif
    }
}
