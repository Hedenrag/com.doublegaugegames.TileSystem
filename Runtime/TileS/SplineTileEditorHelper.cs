#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace DGG.Tiles.Editor
{

    [SelectionBase]
    [RequireComponent(typeof(SplineContainer))]
    public class SplineTileEditorHelper : MonoBehaviour, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public static bool buttonsActive;
#endif

        public GameObject TilePrefab;

        public Tile[,] tiles = new Tile[0, 0];
        public Vector2Int width;

        [SerializeField, HideInInspector] Tile[] savedTiles;
        [SerializeField, HideInInspector] Vector2Int TilesDimension;

        public void OnAfterDeserialize()
        {
            tiles = new Tile[TilesDimension.x, TilesDimension.y];
            for (int i = 0; i < TilesDimension.x; i++)
            {
                for (int j = 0; j < TilesDimension.y; j++)
                {
                    tiles[i, j] = savedTiles[(i * TilesDimension.y) + j];
                }
            }
        }

        public void OnBeforeSerialize()
        {
            TilesDimension = new Vector2Int(tiles.GetLength(0), tiles.GetLength(1));
            savedTiles = new Tile[TilesDimension.x * TilesDimension.y];
            for (int i = 0; i < TilesDimension.x; i++)
            {
                for (int j = 0; j < TilesDimension.y; j++)
                {
                    savedTiles[(i * TilesDimension.y) + j] = tiles[i, j];
                }
            }

        }

        private void OnDrawGizmos()
        {
            if(buttonsActive) return;
            Gizmos.color = Color.green;
            foreach (var tile in tiles)
            {
                Gizmos.matrix = tile.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.8f, 0f, 0.8f));
            }
        }

    }
}
#endif