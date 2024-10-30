using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGG.Tiles
{

    public class TileExtension : MonoBehaviour
    {
        Tile m_tile;
        public Tile tile => m_tile;

        protected virtual void OnValidate()
        {
            Tile tile = GetComponent<Tile>();
            if (tile != null)
            {
                this.m_tile = tile;
            }
            else
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(this);
                };
                Debug.LogError("This gameobject is not a Tile", this.gameObject);
            }
        }
    }

}