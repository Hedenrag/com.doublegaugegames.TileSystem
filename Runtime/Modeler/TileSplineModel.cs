using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Rendering;
using DGG.ExVar;
using System.Linq;
using System.Reflection;

namespace DGG.SimpleModeler
{


    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SplineContainer))]
    public class TileSplineModel : MonoBehaviour
    {
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] SplineContainer splineContainer;

        [SerializeField] public Vector2Int size;

        [SerializeField] public Material[] materials = new Material[0];
        
        [SerializeField, HideInInspector] public SerializableBiDimensionalArray<int> materialIndex = new() ;

        Vector3[] vertexArray;
        int[] facesArray;
        Vector2[] uvsArray;

        private void OnValidate()
        {
            meshFilter = GetComponent<MeshFilter>();
            splineContainer = GetComponent<SplineContainer>();
            if (meshFilter == null) Debug.LogError("Missing mesh filter in GameObject", this);
            if (splineContainer == null) Debug.LogError("Missing splineContainer in GameObject", this);
            if (materials.Length <= 0) materials = new Material[] { new Material(Shader.Find("Unlit/Color")) };
        }

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
                mesh.name = "Spline Mesh";
            }

            if (size.x + size.y <= 0) throw new System.ArgumentOutOfRangeException("size not supported");

            var spline = splineContainer.Spline;

            float length = Mathf.Floor(spline.GetLength());

            List<Vector3> vertex = new();
            List<Vector2> uv = new();

            for (int i = 0; i <= length + 0.5f; i++)
            {
                spline.Evaluate(i / length, out float3 position, out float3 tangent, out float3 upVector);
                var sideDir = Vector3.Cross(upVector, ((Vector3)tangent).normalized);
                for (int x = -size.y; x <= size.x; x++)
                {
                    vertex.Add((Vector3)position + (sideDir * x));
                    uv.Add(new Vector2(x * sideDir.magnitude, i));
                }
            }

            int hsize = size.x + size.y + 1;

            materialIndex = materialIndex.MakeCopyOfSize(hsize, Mathf.RoundToInt(length));
            //RandomMaterials();

            List<int>[] faces = new List<int>[materials.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = new();
            }

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < hsize - 1; j++)
                {
                    List<int> lfaces = faces[materialIndex.array[j, i]];
                    lfaces.Add(((i + 0) * hsize) + j + 0);
                    lfaces.Add(((i + 1) * hsize) + j + 0);
                    lfaces.Add(((i + 0) * hsize) + j + 1);

                    lfaces.Add(((i + 1) * hsize) + j + 0);
                    lfaces.Add(((i + 1) * hsize) + j + 1);
                    lfaces.Add(((i + 0) * hsize) + j + 1);
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
        /// <summary>
        /// Debug only
        /// </summary>
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