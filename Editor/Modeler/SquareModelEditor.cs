using Codice.Client.BaseCommands.Differences;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DGG.SimpleModeler
{
    namespace Editor
    {
        [CustomEditor(typeof(TileSquareModel))]
        public class SquareModelEditor : UnityEditor.Editor
        {

            bool Painting
            {
                get { return painting; }
                set
                {
                    if (painting != value)
                    {
                        painting = value;
                        if (painting)
                        {
                            SceneView.beforeSceneGui += PaintObject;
                        }
                        else
                        {
                            SceneView.beforeSceneGui -= PaintObject;
                        }
                    }
                }
            }

            bool painting;

            int currentPaintLayer;

            private static readonly MethodInfo intersectRayMeshMethod = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            TileSquareModel t;

            RectInt csize;

            private void OnEnable()
            {
                t = (TileSquareModel)target;
                csize = t.size;
                serializedObject.FindProperty("meshFilter").objectReferenceValue = t.GetComponent<MeshFilter>();
                serializedObject.ApplyModifiedProperties();
            }



            public override void OnInspectorGUI()
            {
                if (t.TryGetComponent(Type.GetType("DGG.Tiles.Editor.TileAreaManager, TileSystem"), out var component))
                {
                    if (GUILayout.Button("Copy Size"))
                    {
                        var a = component.GetType();
                        var b = a.GetField("rect", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        var c = b.GetValue(component);
#if UNITY_6000_0_OR_NEWER
                        if (t.size != (RectInt)c)
#else
                        if (t.size.center != ((RectInt)c).center || t.size.size != ((RectInt)c).size)
#endif
                            t.size = (RectInt)c;
                    }
                }

                csize = t.size;
                t.GenerateMesh();

                {
                    bool paintingP = Painting;
                    Painting = GUILayout.Toggle(Painting, EditorGUIUtility.IconContent("paint-brush"), "Button");
                    if (paintingP != Painting)
                    {
                        if (Painting)
                        {
                            SceneView.beforeSceneGui += PaintObject;
                        }
                        else
                        {
                            SceneView.beforeSceneGui -= PaintObject;
                        }
                    }
                }
                if (Painting)
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


            void PaintObject(SceneView sceneView)
            {
                if (currentPaintLayer == -1) return;
                //Debug.Log(painting);
                if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag /*|| Event.current.type == EventType.*/) && (Event.current.button == 0))
                {
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
                        if (t.materialIndex.array.GetLength(0) > uvPos.x - t.size.width &&
                            t.materialIndex.array.GetLength(1) > uvPos.y - t.size.height &&
                            t.materialIndex.array[uvPos.x - t.size.width, uvPos.y - t.size.height] != currentPaintLayer)
                        {
                            t.materialIndex.array[uvPos.x - t.size.width, uvPos.y - t.size.height] = currentPaintLayer;
                            t.GenerateMesh();
                        }
                    }

                    Event.current.Use();
                }
            }

            Vector2[] positions;
            /*
             * 1:
             * 
             * 
             */
            private void OnSceneGUI()
            {
                var rect = t.size;

                if (positions == null || positions.Length == 0)
                {
                    positions = new Vector2[4] { new(), new(), new(), new() };
                }

                Vector2[] vecs = new Vector2[]
                {
                    new Vector2Int(rect.x, rect.y),
                    new Vector2Int(rect.x, rect.height),
                    new Vector2Int(rect.width, rect.height),
                    new Vector2Int(rect.width, rect.y)
                };

                if (IsDiff())
                {
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i] = vecs[i];
                    }
                        positions[0] += new Vector2(0.1f, 0.1f);
                        positions[1] += new Vector2(0.1f, -0.1f);
                        positions[2] += new Vector2(-0.1f, -0.1f);
                        positions[3] += new Vector2(-0.1f, 0.1f);
                }
                bool IsDiff()
                {
                    for (int i = 0; i < positions.Length; i++)
                    {
                        if ((int)positions[i].x != vecs[i].x && (int)positions[i].y != vecs[i].y)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                Vector3[] verts = new Vector3[]
                {
                    GetVertex(positions[0]),
                    GetVertex(positions[1]),
                    GetVertex(positions[2]),
                    GetVertex(positions[3])
                };
                Vector3 GetVertex(Vector2 dir)
                {
                    return t.transform.position + (t.transform.forward * dir.y + (t.transform.right * dir.x)) - t.transform.up * 0.1f;
                }

                Handles.DrawSolidRectangleWithOutline(verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0f, 0f, 0f, 1f));

                Handles.color = Color.blue;

                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i] = (Handles.FreeMoveHandle(verts[i], 0.15f, Vector3.zero, Handles.CircleHandleCap) - verts[i]);
                    if (verts[i].sqrMagnitude != 0f)
                    {
                        var vec2 = new Vector2(Vector3.Dot(t.transform.right, verts[i]), Vector3.Dot(t.transform.forward, verts[i]));
                        switch (i)
                        {
                            case 0:
                                positions[0] += vec2;                            // 3      y       0
                                positions[1].x += vec2.x;                        //
                                positions[3].y += vec2.y;                        // w              x
                                break;                                           //
                            case 1:                                              // 2      h       1
                                positions[1] += vec2;
                                positions[2].y += vec2.y;
                                positions[0].x += vec2.x;
                                break;
                            case 2:
                                positions[2] += vec2;
                                positions[3].x += vec2.x;
                                positions[1].y += vec2.y;
                                break;
                            case 3:
                                positions[3] += vec2;
                                positions[0].y += vec2.y;
                                positions[2].x += vec2.x;
                                break;
                        }
                    }
                }

                rect = new((int)positions[0].x, (int)positions[0].y, (int)positions[2].x, (int)positions[2].y);
                t.size = rect;

            }

        }
    }
}