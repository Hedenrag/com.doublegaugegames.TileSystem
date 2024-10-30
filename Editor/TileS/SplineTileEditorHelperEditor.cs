#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using UnityEditor.Splines;
using Unity.Mathematics;
using UnityEditor.PackageManager;

namespace DGG.Tiles.Editor
{
    [CustomEditor(typeof(SplineTileEditorHelper))]
    public class SplineTileEditorHelperEditor : UnityEditor.Editor
    {
        SplineTileEditorHelper t;
        Transform transform;
        Spline spline;
        Vector2 splineWidth;//x = right y = left

        private void OnEnable()
        {
            t = target as SplineTileEditorHelper;
            transform = t.transform;
            spline = t.GetComponent<SplineContainer>().Spline;
            splineWidth = t.width;
        }

        public override void OnInspectorGUI()
        {
            SplineTileEditorHelper.buttonsActive = GUILayout.Toggle(SplineTileEditorHelper.buttonsActive, "Tiles", "Button");
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            DrawHandles();

            UpdateTilesPosition();
        }

        void UpdateTilesPosition()
        {
            if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) return;

            float length = spline.GetLength();
            int segments = (int)length;
            float distance = length / (float)segments;

            SplinePoint[] positions = GetPositions();



            Vector2Int width = new Vector2Int((int)splineWidth.x, (int)splineWidth.y);
            bool sizeChanged = t.tiles.GetLength(0) != positions.Length || !t.width.Equals(width);
            if (sizeChanged)
            {
                ResizeContainer(width, positions.Length);
            }
            CheckTilesCreated();

            int newWidth = (int)splineWidth.x + (int)splineWidth.y;
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 rightDir = Vector3.Cross(positions[i].forward, positions[i].upVector);
                for (int j = 0; j < newWidth; j++)
                {
                    t.tiles[i, j].transform.SetLocalPositionAndRotation(positions[i].position + (rightDir * (j + 0.5f - ((int)splineWidth.x))), Quaternion.LookRotation(positions[i].forward, positions[i].upVector));
                }
            }
            MakeConnections();

            SplinePoint[] GetPositions()
            {
                List<SplinePoint> list = new();
                for (float i = 0f; i <= length - 0.5f; i += distance)
                {
                    spline.Evaluate((i + 0.5f) / length, out float3 position, out float3 tangent, out float3 upVector);
                    list.Add(new(position, tangent, upVector));
                }
                return list.ToArray();
            }
            if (sizeChanged)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                Undo.RegisterFullObjectHierarchyUndo(t.gameObject, "Changed Tile Bounds");
            }

        }
        struct SplinePoint
        {
            public Vector3 position;
            public Vector3 forward;
            public Vector3 upVector;
            public SplinePoint(Vector3 pos, Vector3 tangent, Vector3 up)
            {
                position = pos;
                forward = tangent.normalized;
                upVector = up;
            }
        }

        void ResizeContainer(Vector2 newWidths, int newLength)
        {
            try
            {
                int newWidth = (int)newWidths.x + (int)newWidths.y;
                Tile[,] newTiles = new Tile[newLength, newWidth];

                int oldWidth = t.tiles.GetLength(1);
                int oldLength = t.tiles.GetLength(0);

                for (int i = 0; i < newLength; i++)
                {
                    for (int j = 1 - (int)newWidths.x; j < newWidths.y; j++)
                    {
                        if (oldLength > i && oldWidth > j + newWidths.x && j + t.width.x >= 0)
                        {
                            newTiles[i, j + (int)newWidths.x] = t.tiles[i, j + (int)newWidths.x];
                            t.tiles[i, j + (int)newWidths.x] = null;
                        }
                    }
                }
                for (int i = 0; i < oldLength; i++)
                {
                    for (int j = 0; j < oldWidth; j++)
                    {
                        Debug.Log($"accessing to destroy [{i},{j}]");
                        if (t.tiles[i, j] != null)
                        {
                            DestroyImmediate(t.tiles[i, j].gameObject);
                        }
                    }
                }

                t.tiles = newTiles;
                t.width = new Vector2Int((int)(newWidths.x), (int)(newWidths.y));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        void CheckTilesCreated()
        {
            for (int i = 0; i < t.tiles.GetLength(0); i++)
            {
                for (int j = 0; j < t.tiles.GetLength(1); j++)
                {
                    if (t.tiles[i, j] == null)
                    {
                        t.tiles[i, j] = ((GameObject)PrefabUtility.InstantiatePrefab(t.TilePrefab, transform)).GetComponent<Tile>();
                        Undo.RegisterCreatedObjectUndo(t.tiles[i, j].gameObject, "Tile Created");
                    }
                }
            }
        }

        void MakeConnections()
        {
            var newTiles = t.tiles;

            var maxi = newTiles.GetLength(0);
            var maxj = newTiles.GetLength(1);

            for (int i = 0; i < maxi; i++)
            {
                for (int j = 0; j < maxj; j++)
                {
                    if (i - 1 > 0) newTiles[i, j].SetConnection(newTiles[i - 1, j]);
                    if (i + 1 < maxi) newTiles[i, j].SetConnection(newTiles[i + 1, j]);
                    if (j - 1 > 0) newTiles[i, j].SetConnection(newTiles[i, j - 1]);
                    if (j + 1 < maxj) newTiles[i, j].SetConnection(newTiles[i, j + 1]);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(newTiles[i, j]);
                }
            }
        }

        void DrawHandles()
        {
            foreach (var knot in spline.Knots)
            {
                DrawHandle(false, knot.Position, knot.Rotation);
                DrawHandle(true, knot.Position, knot.Rotation);
            }
        }

        void DrawHandle(bool right, Vector3 position, Quaternion rotation)
        {
            Vector3 dir = right ? Vector3.right : Vector3.left;
            Vector3 offset = transform.localToWorldMatrix.MultiplyPoint(position + rotation * (dir * ((right ? splineWidth.x : splineWidth.y) + 0.5f)));
            Vector3 newPos = CustomHandles.MoveHandle(offset, Quaternion.LookRotation(transform.localToWorldMatrix.rotation * rotation * dir, rotation * Vector3.up), 0.5f, Vector3.one, Handles.ConeHandleCap, new Color32(0x00, 0xFF, 0x93, 0xFF));

            float distance = Vector3.Dot(rotation * (t.transform.right * (right ? 1f : -1f)), newPos - offset);


            if (right) splineWidth.x += distance;
            else splineWidth.y += distance;
        }
        void DrawPositionHandles()
        {
            for (int i = 0; i < t.tiles.GetLength(0); i++)
            {
                for (int j = 0; j < t.tiles.GetLength(1); j++)
                {
                    Handles.Label(t.tiles[i, j].transform.position, $"[{i}, {j}]");
                }
            }
        }
    }
}
#endif