using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DGG.Tiles
{
    [Serializable]
    public enum TileDirection
    {
        Forward,
        Back,
        Left,
        Right
    }
    public static class TileEnumsOperators
    {
        public static TileDirection ToTileDir(this AxisDir axisDir)
        {
            switch (axisDir)
            {
                case AxisDir.Z: return TileDirection.Forward;
                case AxisDir.X: return TileDirection.Right;
                case AxisDir.NZ: return TileDirection.Back;
                case AxisDir.NX: return TileDirection.Left;
            }
            throw new ArgumentOutOfRangeException();
        }
        public static AxisDir ToAxisDir(this TileDirection tileDir)
        {
            return tileDir switch
            {
                TileDirection.Forward => AxisDir.Z,
                TileDirection.Right => AxisDir.X,
                TileDirection.Left => AxisDir.NX,
                TileDirection.Back => AxisDir.NZ,
                _ => AxisDir.Y
            };
        }

        public static AxisDir ClosestDir(this Vector2 dir)
        {
            float fwd = Vector2.Dot(dir, Vector2.up);
            float rht = Vector2.Dot(dir, Vector2.right);

            if(Mathf.Abs(fwd)>=Mathf.Abs(rht))
            {
                if (float.IsNegative(fwd))
                {
                    return AxisDir.NZ;
                }
                else
                {
                    return AxisDir.Z;
                }
            }
            else
            {
                if (float.IsNegative(rht))
                {
                    return AxisDir.NX;
                }
                else
                {
                    return AxisDir.X;
                }
            }
        }

        public static AxisDir ClosestAxis(this Transform transform, Vector2 dir)
        {
            var fwdDir = transform.GetPlaneForward();
            var rhtDir = new Vector2(fwdDir.y, -fwdDir.x);

            float fwd = Vector2.Dot(fwdDir, dir);
            float rht = Vector2.Dot(rhtDir, dir);

            float absFwd = Mathf.Abs(fwd);
            float absRht = Mathf.Abs(rht);

            if(absFwd > absRht)
            {
                if (float.IsNegative(fwd))
                {
                    return AxisDir.NZ;
                }
                return AxisDir.Z;
            }
            else
            {
                if (float.IsNegative(rht))
                {
                    return AxisDir.NX;
                }
                return AxisDir.X;
            }
        }

        public static AxisDir ClosestAxis(this Transform transform, Vector3 dir)
        {
            float fwd = Vector3.Dot(transform.forward, dir);
            float right = Vector3.Dot(transform.right, dir);
            float up = Vector3.Dot(transform.up, dir);

            float absFwd = Mathf.Abs(fwd);
            float absRight = Mathf.Abs(right);
            float absUp = Mathf.Abs(up);

            if(absFwd > absRight && absFwd > absUp)
            {
                if (float.IsNegative(fwd))
                {
                    return AxisDir.NZ;
                }
                return AxisDir.Z;
            }else if(absRight > absUp)
            {
                if (float.IsNegative(right))
                {
                    return AxisDir.NX;
                }
                return AxisDir.X;
            }else
            {
                if (float.IsNegative(up))
                {
                    return AxisDir.NY;
                }
                return AxisDir.Y;
            }

        }

        public static Vector2 GetPlane(this Vector3 dir, Vector3 up)
        {
            Vector3 nrm = Quaternion.FromToRotation(up, Vector3.up) * dir;
            return new Vector2(nrm.x, nrm.z);
        }
        public static Vector2 GetPlaneForward(this Transform transform)
        { 
            return transform.forward.GetPlane(transform.up);
        }

        public static Vector3 GetAxis(this Transform transform, AxisDir axis)
        {
            return axis switch
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
    }
    [Serializable]
    public enum AxisDir
    {
        X,
        Y,
        Z,
        NX,
        NY,
        NZ
    }

    
}