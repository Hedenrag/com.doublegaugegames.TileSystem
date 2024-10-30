using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGG.Tiles
{
    public class TileObjects : MonoBehaviour
    {
        public Tile tile => m_tile;
        [SerializeField] protected Tile m_tile;

        protected virtual void Awake()
        {
            m_tile = GetComponentInParent<Tile>();
            if (m_tile == null)
            {
                m_tile = Tile.FindNearestTile(transform.position);
            }
            transform.position = m_tile.transform.position;

            if(m_tile.tileObject == null)
            {
                m_tile.tileObject = this;
            }
            if(this is TileEntity t)
            {
                m_tile.tileEntity = t;
            }
        }
        protected virtual void Start()
        {
            
        }
    }

}