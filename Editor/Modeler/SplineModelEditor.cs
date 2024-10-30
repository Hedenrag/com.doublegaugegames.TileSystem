using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Splines;
using UnityEngine;
using Unity.Mathematics;

namespace DGG.SimpleModeler
{

    namespace Editor
    {

        [CustomEditor(typeof(TileSplineModel))]
        public class SplineModelEditor : UnityEditor.Editor
        {
            bool painting;

            int currentPaintLayer;

            private static readonly MethodInfo intersectRayMeshMethod = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            TileSplineModel t;

            SplineContainer splineContainer;
            Vector2Int csize;

            private void OnEnable()
            {
                t = (TileSplineModel)target;
                splineContainer = t.GetComponent<SplineContainer>();
                serializedObject.FindProperty("splineContainer").objectReferenceValue = t.GetComponent<SplineContainer>();
                serializedObject.FindProperty("meshFilter").objectReferenceValue = t.GetComponent<MeshFilter>();
                Spline.Changed += MarkAsSplineEdited;
                csize = t.size;
            }

            private void MarkAsSplineEdited(Spline s, int n, SplineModification sm)
            {
                if (s != splineContainer.Spline) return;

                t.GenerateMesh();
            }
            public override void OnInspectorGUI()
            {
                if (csize != t.size)
                {
                    if (t.size.x + t.size.y - 1 <= 0)
                    {
                        if (t.size.x < t.size.y)
                        {
                            t.size.x = 1 - t.size.y;
                        }
                        else
                        {
                            t.size.y = 1 - t.size.x;
                        }
                    }
                    csize = t.size;
                    t.GenerateMesh();
                }
                painting = GUILayout.Toggle(painting, EditorGUIUtility.IconContent("paint-brush"), "Button");
                if (painting)
                {
                    currentPaintLayer = EditorGUILayout.Popup(currentPaintLayer, t.materials.Select(x => x.name).ToArray());
                    for (int i = 0; i < t.materials.Length; i++)
                    {
                        if (GUILayout.Button(AssetPreview.GetAssetPreview(t.materials[i])))
                        {
                            currentPaintLayer = i;
                        }
                    }

                }
                base.OnInspectorGUI();
                if (GUILayout.Button("Generate"))
                {
                    t.GenerateMesh();
                }
                if (GUILayout.Button("Random Materials"))
                {
                    t.RandomMaterials();
                    t.GenerateMesh();
                }
            }



            private void OnSceneGUI()
            {
                if (painting)
                {
                    PaintTexture();
                }

                Size();

            }

            private void PaintTexture()
            {
                if (currentPaintLayer == -1) return;
                //Debug.Log(painting);
                if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
                {
                    Debug.Log("mouse");
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    Mesh sharedMesh = t.GetComponent<MeshFilter>().sharedMesh;

                    object[] rayMeshParameters = new object[] { ray, sharedMesh, t.transform.localToWorldMatrix, null };

                    if ((bool)intersectRayMeshMethod.Invoke(null, rayMeshParameters))
                    {
                        RaycastHit hit = (RaycastHit)rayMeshParameters[3];

                        Vector2 p0 = sharedMesh.uv[sharedMesh.triangles[hit.triangleIndex * 3 + 0]];
                        Vector2 p1 = sharedMesh.uv[sharedMesh.triangles[hit.triangleIndex * 3 + 1]];
                        Vector2 p2 = sharedMesh.uv[sharedMesh.triangles[hit.triangleIndex * 3 + 2]];

                        Vector2 uv = (p0 + p1 + p2) / 3f;

                        Vector2Int uvPos = new(Mathf.FloorToInt(uv.x), Mathf.FloorToInt(uv.y));
                        Debug.Log(uvPos);
                        Debug.Log(t.size);
                        if (t.materialIndex.array.GetLength(0) > uvPos.x + t.size.y &&
                            t.materialIndex.array.GetLength(1) > uvPos.y &&
                            t.materialIndex.array[uvPos.x + t.size.y, uvPos.y] != currentPaintLayer)
                        {
                            t.materialIndex.array[uvPos.x + t.size.y, uvPos.y] = currentPaintLayer;
                            t.GenerateMesh();
                        }
                    }

                    Event.current.Use();
                }
            }

            Vector2 floatPositions = new();

            void Size()
            {

                if ((int)floatPositions.x != t.size.x || (int)floatPositions.y != t.size.y)
                {
                    floatPositions.x = t.size.x + 0.1f;
                    floatPositions.y = t.size.y + 0.1f;
                }

                splineContainer.Spline.Evaluate(0.5f, out float3 position, out float3 tangent, out float3 upVector);

                Vector3 right = Vector3.Cross(t.transform.up, Vector3.Normalize(tangent));
                for (int i = 0; i < 2; i++)
                {
                    if (i == 1) right *= -1f;
                    Vector3 oldPos = ((Vector3)position) + (right * t.size.y);
                    Vector3 newPos = Handles.FreeMoveHandle(oldPos, 1f, Vector3.zero, Handles.RectangleHandleCap);
                    Vector3 offset = oldPos - newPos;
                    if (i == 0)
                    {
                        floatPositions.x += Vector3.Dot(offset, right);
                    }
                    else
                    {
                        floatPositions.y += Vector3.Dot(offset, right);
                    }
                }
            }
        }

    }
}