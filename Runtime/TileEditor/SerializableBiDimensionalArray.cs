using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGG
{
    namespace ExVar
    {

        [System.Serializable]
        public class SerializableBiDimensionalArray<T> : ISerializationCallbackReceiver
        {
            public T[,] array = new T[0, 0];

            [SerializeField, HideInInspector] T[] savedTiles;
            [SerializeField, HideInInspector] Vector2Int TilesDimension;



            public void OnAfterDeserialize()
            {
                array = new T[TilesDimension.x, TilesDimension.y];
                for (int i = 0; i < TilesDimension.x; i++)
                {
                    for (int j = 0; j < TilesDimension.y; j++)
                    {
                        array[i, j] = savedTiles[(i * TilesDimension.y) + j];
                    }
                }
            }

            public void OnBeforeSerialize()
            {
                TilesDimension = new Vector2Int(array.GetLength(0), array.GetLength(1));
                savedTiles = new T[TilesDimension.x * TilesDimension.y];
                for (int i = 0; i < TilesDimension.x; i++)
                {
                    for (int j = 0; j < TilesDimension.y; j++)
                    {
                        savedTiles[(i * TilesDimension.y) + j] = array[i, j];
                    }
                }

            }

            public SerializableBiDimensionalArray<T> MakeCopyOfSize(int x, int y)
            {
                var t = new SerializableBiDimensionalArray<T>();

                t.array = new T[x, y];

                var sizex = array.GetLength(0);
                var sizey = array.GetLength(1);

                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (sizex > i && sizey > j)
                        {
                            t.array[i, j] = array[i, j];
                        }
                        else
                        {
                            t.array[i, j] = default;
                        }
                    }
                }
                return t;
            }

            public SerializableBiDimensionalArray<T> MakeCopyTo(int sizeX, int sizeY, int copyStartX = 0, int copyStartY = 0)
            {
                var t = new SerializableBiDimensionalArray<T>();

                t.array = new T[sizeX, sizeY];

                var sizex = array.GetLength(0);
                var sizey = array.GetLength(1);

                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        if (sizex > i + copyStartX && sizey > j + copyStartY)
                        {
                            t.array[i, j] = array[i + copyStartX, j + copyStartY];
                        }
                    }
                }
                return t;
            }

            public SerializableBiDimensionalArray() { }

            public SerializableBiDimensionalArray(T[,] values)
            {
                array = values;
            }

        }
    }
}