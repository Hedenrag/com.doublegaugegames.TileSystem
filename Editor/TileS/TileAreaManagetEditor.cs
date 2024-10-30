#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DGG.Tiles.Editor
{


    [CustomEditor(typeof(TileAreaManager))]
    public class TileAreaManagetEditor : UnityEditor.Editor
    {
        TileAreaManager t;
        Transform transform;
        Rect rect;

        private void OnEnable()
        {
            t = target as TileAreaManager;
            transform = t.transform;
            rect = new Rect(t.rect.position + (Vector2.one * 0.1f), t.rect.size + (Vector2.one * -0.1f));
        }

        public override void OnInspectorGUI()
        {
            SplineTileEditorHelper.buttonsActive = GUILayout.Toggle(SplineTileEditorHelper.buttonsActive, "Tiles", "Button");
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            t = target as TileAreaManager;

            Vector3[] verts = new Vector3[]
            {
                GetVertex(new Vector2(rect.x, rect.y)),
                GetVertex(new Vector2(rect.x, rect.height)),
                GetVertex(new Vector2(rect.width, rect.height)),
                GetVertex(new Vector2(rect.width, rect.y))
            };

            Handles.DrawSolidRectangleWithOutline(verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0f, 0f, 0f, 1f));

            DragExpansion(TileDirection.Forward);
            DragExpansion(TileDirection.Right);
            DragExpansion(TileDirection.Back);
            DragExpansion(TileDirection.Left);


            RectInt roundedBounds = new RectInt((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            if (!t.rect.Equals(roundedBounds))
            {
                //Debug.Log("different bounds");
                //Set new rect

                Tile[,] newTiles = new Tile[roundedBounds.x - roundedBounds.width, roundedBounds.y - roundedBounds.height];


                for (int i = Mathf.Min(roundedBounds.width, t.rect.width); i < Mathf.Max(roundedBounds.x, t.rect.x); i++)
                {
                    for (int j = Mathf.Min(roundedBounds.height, t.rect.height); j < Mathf.Max(roundedBounds.y, t.rect.y); j++)
                    {
                        if (InRectBounds(i, j, t.rect) && InRectBounds(i, j, roundedBounds))
                        {

                            newTiles[i - roundedBounds.width - 1, j - roundedBounds.height - 1] = t.tiles[i - t.rect.width - 1, j - t.rect.height - 1];
                            t.tiles[i - t.rect.width - 1, j - t.rect.height - 1] = null;
                        }
                    }
                }
                for (int i = 0; i < t.tiles.GetLength(0); i++)
                {
                    for (int j = 0; j < t.tiles.GetLength(1); j++)
                    {
                        if (t.tiles[i, j] != null)
                        {
                            DestroyImmediate(t.tiles[i, j].gameObject);
                        }
                    }
                }
                {
                    int maxi = newTiles.GetLength(0);
                    int maxj = newTiles.GetLength(1);
                    for (int i = 0; i < maxi; i++)
                    {
                        for (int j = 0; j < maxj; j++)
                        {
                            if (newTiles[i, j] == null)
                            {
                                newTiles[i, j] =((GameObject)PrefabUtility.InstantiatePrefab(t.TilePrefab, transform)).GetComponent<Tile>();
                                newTiles[i, j].transform.localPosition = new Vector3(i + roundedBounds.width + 0.5f, 0f, j + roundedBounds.height + 0.5f);
                            }
                        }
                    }
                    for (int i = 0; i < maxi; i++)
                    {
                        for (int j = 0; j < maxj; j++)
                        {
                            if (i - 1 > 0) newTiles[i, j].SetConnection(newTiles[i - 1, j]);
                            if (i + 1 < maxi) newTiles[i, j].SetConnection(newTiles[i + 1, j]);
                            if (j - 1 > 0) newTiles[i, j].SetConnection(newTiles[i, j - 1]);
                            if (j + 1 < maxj) newTiles[i, j].SetConnection(newTiles[i, j + 1]);
                        }
                    }
                }

                t.tiles = newTiles;

                t.rect = roundedBounds;
                Undo.RegisterFullObjectHierarchyUndo(t.gameObject, "Changed Tile Bounds");
            }
        }
        bool InRectBounds(int x, int y, RectInt rect) { return InRectBound(x, new Vector2Int(rect.x, rect.width)) && InRectBound(y, new Vector2Int(rect.y, rect.height)); }
        bool InRectBound(int n, Vector2Int rectAxis) { return n > rectAxis.y && n < rect.x; }
        Vector3 GetVertex(Vector2 dir)
        {
            return t.transform.position + (t.transform.forward * dir.y + (t.transform.right * dir.x)) - transform.up * 0.1f;
        }

        void DragExpansion(TileDirection tileDirection)
        {
            Vector3 offset = Get3DDir(tileDirection) * (GetSize(tileDirection) + 0.3f) + transform.position + (transform.up*0.2f);

            //Handles.FreeMoveHandle(transform.position, 1f, Vector3.one, Handles.ConeHandleCap);
            Vector3 newPos = CustomHandles.MoveHandle(offset, Quaternion.LookRotation(Get3DDir(tileDirection), transform.up), 0.5f, Vector3.one, Handles.ConeHandleCap, Color.blue);

            newPos -= offset;
            switch (tileDirection)
            {
                case TileDirection.Forward:
                    rect.y += Vector3.Dot(newPos, transform.forward);
                    break;
                case TileDirection.Right:
                    rect.x += Vector3.Dot(newPos, transform.right);
                    break;
                case TileDirection.Back:
                    rect.height += Vector3.Dot(newPos, transform.forward);
                    break;
                case TileDirection.Left:
                    rect.width += Vector3.Dot(newPos, transform.right);
                    break;
            }

            Vector3 Get3DDir(TileDirection dir)
            {
                return dir switch
                {
                    TileDirection.Forward => transform.forward,
                    TileDirection.Right => transform.right,
                    TileDirection.Back => -transform.forward,
                    TileDirection.Left => -transform.right,
                    _ => Vector3.zero
                };
            }
            float GetSize(TileDirection dir)
            {
                return dir switch
                {
                    TileDirection.Forward => rect.y,
                    TileDirection.Right => rect.x,
                    TileDirection.Back => -rect.height,
                    TileDirection.Left => -rect.width,
                    _ => 0f,
                };
            }
        }

    }
}
#endif