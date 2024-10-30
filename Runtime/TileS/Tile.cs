using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

[assembly: InternalsVisibleTo("TileSystemEditor")]
[assembly: InternalsVisibleTo("Modeler")]
namespace DGG
{
    namespace Tiles
    {
        [DefaultExecutionOrder(-5)]
        public class Tile : MonoBehaviour
        {
            [SerializeField]protected Tile connectionForward;
            [SerializeField]protected Tile connectionBack;
            [SerializeField]protected Tile connectionLeft;
            [SerializeField]protected Tile connectionRight;

            [Space(15f)]
            [SerializeField] internal TileObjects tileObject;
            public TileObjects TileObject => tileObject;
            [Space(5f)]
            [SerializeField] internal TileEntity tileEntity;
            public TileEntity TileEntity => tileEntity;

            public UnityEvent<TileEntity> OnTileEnter;
            public UnityEvent<TileEntity> OnTileExit;

            static List<Tile> tiles = new List<Tile>();

            public Action<TileEntity> OnTileInteract;

            protected virtual void Awake()
            {
                tiles.Add(this);
            }
            protected virtual void OnDestroy()
            {
                tiles.Remove(this);
            }

            public static Tile FindNearestTile(Vector3 position)
            {
                Tile nearestTile = null;
                float distance = float.PositiveInfinity;
                for (int i = 0; i < tiles.Count; i++)
                {
                    float cDist;
                    if ((cDist = Vector3.SqrMagnitude(tiles[i].transform.position - position)) < distance)
                    {
                        distance = cDist;
                        nearestTile = tiles[i];
                    }
                }
                if (nearestTile == null) throw new Exception("No tiles created");
                return nearestTile;
            }

            public Tile GetNeighborTile(TileDirection direction)
            {
                return direction switch
                {
                    TileDirection.Forward => connectionForward,
                    TileDirection.Back => connectionBack,
                    TileDirection.Left => connectionLeft,
                    TileDirection.Right => connectionRight,
                    _ => null,
                };
            }

            protected ref Tile GetTileRef(TileDirection direction)
            {
                switch (direction)
                {
                    case TileDirection.Forward: return ref connectionForward;
                    case TileDirection.Back: return ref connectionBack;
                    case TileDirection.Left: return ref connectionLeft;
                    case TileDirection.Right: return ref connectionRight;
                };
                throw new ArgumentOutOfRangeException();
            }

            public void SetConnection(Tile tile)
            {
                GetTileRef(GetTransformLocalDir(tile.transform.position).ToTileDir()) = tile;
                tile.GetTileRef(tile.GetTransformLocalDir(transform.position).ToTileDir()) = this;
            }
            /// <summary>
            /// Get the direction of the object from the Tile point of view 
            /// </summary>
            /// <param name="dir"></param>
            /// <returns></returns>
            public AxisDir GetTransformLocalDir(Vector3 dir)
            {
                var meHimDir = dir - transform.position;

                var fwdAllignment = Vector3.Dot(transform.forward, meHimDir);
                var rightAllignment = Vector3.Dot(transform.right, meHimDir);

                if (Mathf.Abs(fwdAllignment) > Mathf.Abs(rightAllignment))
                {
                    if (!float.IsNegative(fwdAllignment))
                    {
                        return AxisDir.Z;
                    }
                    return AxisDir.NZ;
                }
                else
                {
                    if (!float.IsNegative(rightAllignment))
                    {
                        return AxisDir.X;
                    }
                    return AxisDir.NX;
                }
            }

            /// <summary>
            /// Get the World-Space direction of the object from the tile point of view
            /// </summary>
            /// <param name="pos"></param>
            /// <returns></returns>
            public AxisDir GetTransformDir(Vector3 pos)
            {
                Vector3 dir = pos - transform.position;
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
                {
                    if (!float.IsNegative(dir.x))
                    {
                        return AxisDir.X;
                    }
                    return AxisDir.NX;
                }
                if (!float.IsNegative(dir.z))
                {
                    return AxisDir.Z;
                }
                return AxisDir.NZ;
            }
            public Vector3 GetLocalAxis(AxisDir dir)
            {
                return dir switch
                {
                    AxisDir.X => transform.right,
                    AxisDir.Y => transform.up,
                    AxisDir.Z => transform.forward,
                    AxisDir.NX => -transform.right,
                    AxisDir.NY => -transform.up,
                    AxisDir.NZ => -transform.forward,
                    _ => Vector3.zero
                };
            }

            public void Interact(TilePlayerEntity player)
            {
                OnTileInteract?.Invoke(player);
            }
        }
    }
}