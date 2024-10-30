using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using DGG.ExVar;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

[assembly: InternalsVisibleTo("TileSystemEditor")]
[assembly: InternalsVisibleTo("TileSystem")]
[assembly: InternalsVisibleTo("Modeler")]
[assembly: InternalsVisibleTo("ModelerEditor")]
namespace DGG
{
    namespace SimpleModeler
    {
        [RequireComponent(typeof(MeshFilter))]
        [RequireComponent(typeof(MeshRenderer))]
        public class TileSquareModel : MonoBehaviour
        {
            [SerializeField] MeshFilter meshFilter;

            public RectInt Size { get { return size; } set { size = value; GenerateMesh(); } }
            [SerializeField] internal RectInt size;

            [SerializeField] public Material[] materials;

            [SerializeField, HideInInspector] public SerializableBiDimensionalArray<int> materialIndex = new();

            Vector3[] vertexArray;
            int[] facesArray;
            Vector2[] uvsArray;

            private void Start()
            {
                GenerateMesh();
            }

            public void GenerateMesh()
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    mesh = new Mesh();
                    mesh.name = "Square Mesh";
                }

                if(materials.Length == 0) { materials = new Material[] { new Material(Shader.Find("Unlit/Color"))}; }

                Vector2Int rsize = new Vector2Int(Size.x - Size.width, Size.y - Size.height);

                if (rsize.x < 0 || rsize.y < 0) throw new System.ArgumentOutOfRangeException("size not supported");

                List<Vector3> vertex = new();
                List<Vector2> uv = new();

                for (int i = 0; i < rsize.x + 1; i++)
                {
                    for (int j = 0; j < rsize.y + 1; j++)
                    {
                        vertex.Add(new Vector3(i + Size.width, 0f, j + Size.height));
                        uv.Add(new Vector2(i + Size.width, j + Size.height));

                        //Debug.Log($"{i}+{Size.width}, {j}+{Size.height} = {vertex.Last()}");
                    }
                }

                materialIndex = materialIndex.MakeCopyOfSize(rsize.x, rsize.y);

                List<int>[] faces = new List<int>[materials.Length];
                for (int i = 0; i < faces.Length; i++) faces[i] = new List<int>();
                int ni = 0, nj = 0;

                for (int i = 0; i < rsize.x; i++)
                {
                    ni = i;
                    for (int j = 0; j < rsize.y; j++)
                    {
                        nj = j;
                        int a = materialIndex.array[i, j];
                        List<int> lfaces = faces[a];
                        lfaces.Add(((i + 1) * (rsize.y + 1)) + j + 0);
                        lfaces.Add(((i + 0) * (rsize.y + 1)) + j + 0);
                        lfaces.Add(((i + 0) * (rsize.y + 1)) + j + 1);

                        lfaces.Add(((i + 1) * (rsize.y + 1)) + j + 1);
                        lfaces.Add(((i + 1) * (rsize.y + 1)) + j + 0);
                        lfaces.Add(((i + 0) * (rsize.y + 1)) + j + 1);

                        //Debug.Log($"{i},{j}");
                    }
                }


                mesh.Clear();

                SubMeshDescriptor[] subMeshes = new SubMeshDescriptor[faces.Length];
                int totalF = 0;
                foreach (var face in faces) { totalF += face.Count; }
                int[] allfaces = new int[totalF];
                int pastIndexes = 0;
                for (int i = 0; i < subMeshes.Length; i++)
                {
                    subMeshes[i] = new SubMeshDescriptor(pastIndexes, faces[i].Count);
                    //Debug.Log($"Submesh {i} has {faces[i].Count / 6} faces");
                    for (int j = 0; j < faces[i].Count; j++)
                    {
                        allfaces[j + pastIndexes] = faces[i][j];
                    }
                    pastIndexes += faces[i].Count;
                }


                uvsArray = uv.ToArray();
                vertexArray = vertex.ToArray();
                facesArray = allfaces;

                mesh.vertices = vertex.ToArray();
                mesh.uv = uvsArray;
                mesh.triangles = allfaces;
                mesh.SetSubMeshes(subMeshes);

                //mesh.Optimize();

                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();


                meshFilter.sharedMesh = mesh;
                GetComponent<MeshRenderer>().materials = materials;
                Debug.Log(materials.Length);
            }

            public void RandomMaterials()
            {
                for (int i = 0; i < materialIndex.array.GetLength(0); i++)
                {
                    for (int j = 0; j < materialIndex.array.GetLength(1); j++)
                    {
                        materialIndex.array[i, j] = UnityEngine.Random.Range(0, materials.Length);
                    }
                }
            }
#if UNITY_EDITOR
            List<Color> colors = new List<Color>();
            internal void Draw()
            {
                if (vertexArray == null) return;
                if (facesArray == null) return;

                Handles.matrix = transform.localToWorldMatrix;

                for (int i = 0; i < facesArray.Length / 3; i++)
                {

                    int j = i * 3;
                    Handles.color = Rnd();
                    Handles.DrawAAConvexPolygon(new Vector3[] { vertexArray[facesArray[j]], vertexArray[facesArray[j + 1]], vertexArray[facesArray[j + 2]] });
                    //Gizmos.DrawLineStrip(stackalloc Vector3[] { vertexArray[facesArray[j]], vertexArray[facesArray[j + 1]], vertexArray[facesArray[j + 2]] }, false);

                    Color Rnd()
                    {
                        if (colors.Count > i) { return colors[i]; }
                        colors.Add(new Color(Nrd(), Nrd(), Nrd()));
                        return Rnd();
                        float Nrd()
                        {
                            return UnityEngine.Random.Range(0.0f, 1.0f);
                        }
                    }

                }
                Handles.color = Color.black;
                for (int i = 0; i < vertexArray.Length; i++)
                {
                    Handles.Label(vertexArray[i], $"[{uvsArray[i].x}, {uvsArray[i].y}]");
                }
            }
#endif
        }
    }
}