#if UNITY_EDITOR
using DGG.Tiles.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DGG.Tiles.Editor
{

    /// <summary>
    /// this script draws the gizmos of the tiles and allows connecting them.
    /// It will not exist in runtime
    /// </summary>
    [RequireComponent(typeof(Tile))]
    [ExecuteAlways]
    public class TileEditorHelper : MonoBehaviour
    {
        int id;
        static Dictionary<int, GameObject> HandleIDs = new();
        Tile tile;

        private void Start()
        {
            if (Application.isPlaying)
                Destroy(this);
        }

        private void OnEnable()
        {
            tile = GetComponent<Tile>();
            id = gameObject.GetInstanceID();
            HandleIDs.Add(id, gameObject);
            SceneView.duringSceneGui += OnSceneGUI;
        }
        private void OnDisable()
        {
            HandleIDs.Remove(id);
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!SplineTileEditorHelper.buttonsActive) return;
            Vector3 cameraPos = Camera.current.WorldToViewportPoint(transform.position);
            if (cameraPos.z < 0f || cameraPos.x < 0f || 1f < cameraPos.x || cameraPos.y < 0f || 1f < cameraPos.y) return;
            DrawArrows();

            TileButtonHandle(transform.position, Quaternion.LookRotation(transform.up, transform.forward), 0.40f, new Color32(0x32, 0xE6, 0x00, 0xF0), sceneView);
        }
        void DrawArrows()
        {
            Matrix4x4 matrix = Handles.matrix;
            Color color = Handles.color;
            Handles.matrix = Matrix4x4.identity;
            Handles.color = new Color32(0x33, 0xFF, 0xA9, 0xFF);
            DrawArrow(TileDirection.Forward);
            DrawArrow(TileDirection.Right);
            DrawArrow(TileDirection.Back);
            DrawArrow(TileDirection.Left);
            Handles.matrix = matrix;
            Handles.color = color;

        }
        void DrawArrow(TileDirection direction)
        {
            Tile t = tile.GetNeighborTile(direction);
            if (t == null) return;
            var targetDir = t.GetLocalAxis(t.GetTransformLocalDir(transform.position));
            var originDir = tile.GetLocalAxis(direction.ToAxisDir());
            Handles.DrawPolyLine(ArrowPoints(transform.position + (originDir * 0.2f), t.transform.position + (targetDir * 0.2f)));
        }
        Vector3[] ArrowPoints(Vector3 origin, Vector3 target)
        {
            var dir = (target - origin);
            var right = Vector3.Cross(dir, transform.up);
            Vector3[] points = new Vector3[]
            {
                origin,
                target,
                target - (dir * 0.3f) + (right * 0.3f),
                target - (dir * 0.3f) - (right * 0.3f),
                target,
            };
            return points;
        }

        void TileButtonHandle(Vector3 position, Quaternion rotation, float size, Color color, SceneView sceneView)
        {

            Event current = Event.current;

            switch (current.GetTypeForControl(id))
            {
                case EventType.Layout:
                    Handles.RectangleHandleCap(id, position, rotation, size, EventType.Layout);
                    break;
                case EventType.MouseMove:
                    HandleUtility.Repaint();
                    break;
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == id && !current.alt)
                    {
                        if (current.button == 0)
                        {
                            GUIUtility.hotControl = id;
                            current.Use();
                        }
                        if (current.button == 2)
                        {
                            GUIUtility.hotControl = 0;
                            current.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && (current.button == 0))
                    {
                        int nearestID = HandleUtility.nearestControl;
                        GUIUtility.hotControl = 0;
                        current.Use();
                        if (id == nearestID)
                        {
                            Selection.activeGameObject = gameObject;
                        }
                        else if (HandleIDs.ContainsKey(nearestID))
                        {
                            var g = HandleIDs[nearestID];
                            var ot = g.GetComponent<Tile>();
                            var mt = GetComponent<Tile>();
                            ot.SetConnection(mt);
                            Undo.RecordObject(ot, "Save Tile Connections");
                            Undo.RecordObject(mt, "Save Tile Connections");
                            PrefabUtility.RecordPrefabInstancePropertyModifications(ot);
                            PrefabUtility.RecordPrefabInstancePropertyModifications(mt);
                        }
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == id)
                    {

                        Handles.color = Color.red;
                        Matrix4x4 matrix = Handles.matrix;
                        Handles.matrix = Matrix4x4.identity;
                        Handles.DrawLine(transform.position, sceneView.camera.ScreenToWorldPoint(new Vector3(current.mousePosition.x, (sceneView.cameraViewport.height) - current.mousePosition.y, sceneView.cameraDistance)), 2f);
                        Handles.matrix = matrix;
                        Handles.color = Color.Lerp(color, Color.yellow, 0.6f);
                        Handles.RectangleHandleCap(id, position, rotation, size, EventType.Repaint);
                        Handles.color = color;
                    }
                    else
                    {
                        Handles.color = color;
                        if (HandleUtility.nearestControl == id)
                        {
                            if (GUI.enabled && GUIUtility.hotControl == 0 && !current.alt)
                            {
                                Handles.color = Color.Lerp(color, Color.yellow, 0.9f);
                            }
                            else
                            {
                                Handles.color = Color.Lerp(color, Color.red, 0.8f);
                            }
                        }
                        Handles.RectangleHandleCap(id, position, rotation, size, EventType.Repaint);
                        Handles.color = color;

                    }
                    break;
            }
        }
    }
}
[CustomEditor(typeof(TileEditorHelper))]
public class TileEditorHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SplineTileEditorHelper.buttonsActive = GUILayout.Toggle(SplineTileEditorHelper.buttonsActive, "Tiles", "Button");
        base.OnInspectorGUI();
    }
}

#endif