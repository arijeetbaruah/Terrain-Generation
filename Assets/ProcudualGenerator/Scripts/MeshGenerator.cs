using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProcudualGenerator
{
    public static class MeshGenerator
    {
        public static Mesh mesh;

        public static Mesh GenerateTerrainMesh(NativeArray<float> heightMap, NativeArray<Color> colourMap, TerrainData terrainData, int borderedSize, int levelOfDetail)
        {
            if (MeshGenerator.mesh == null)
            {
                mesh = new Mesh();
            }

            int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
            int verticesPerLine = (borderedSize - 1) / meshSimplificationIncrement + 1;

            NativeArray<Vector3> vertices = new NativeArray<Vector3>(borderedSize * borderedSize, Allocator.TempJob);
            NativeArray<Vector2> uvs = new NativeArray<Vector2>(borderedSize * borderedSize, Allocator.TempJob);
            NativeArray<int> triangles = new NativeArray<int>((borderedSize - 1) * (borderedSize - 1) * 6, Allocator.TempJob);

            MeshJobData meshJobData = new MeshJobData()
            {
                triangles = triangles,
                vertices = vertices,
                uvs = uvs
            };

            NativeArray<float> heightCurveMap = new NativeArray<float>(borderedSize * borderedSize, Allocator.TempJob);

            for (int i = 0; i < borderedSize * borderedSize; i++)
            {
                heightCurveMap[i] = terrainData.meshHeightCurve.Evaluate(heightMap[i]);
            }

            MeshGeneratorJob meshGeneratorJob = new MeshGeneratorJob()
            {
                heightMap = heightMap,
                heightMultiplier = terrainData.meshHeightMultiplier,
                borderedSize = borderedSize,
                heightCurve = heightCurveMap,
                levelOfDetail = levelOfDetail,
                meshSimplificationIncrement = meshSimplificationIncrement,
                verticesPerLine = verticesPerLine,
                meshData = meshJobData,
            };

            JobHandle jobHandle = meshGeneratorJob.Schedule(heightMap.Length, meshSimplificationIncrement);

            jobHandle.Complete();
            meshGeneratorJob.meshData.CreateMesh(colourMap, ref mesh);

            vertices.Dispose();
            uvs.Dispose();
            triangles.Dispose();
            heightCurveMap.Dispose();

            return mesh;
        }
    }

    [BurstCompile]
    public struct MeshGeneratorJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> heightMap;
        [ReadOnly] public float heightMultiplier;
        [ReadOnly] public int borderedSize;
        [ReadOnly] public NativeArray<float> heightCurve;
        [ReadOnly] public int levelOfDetail;
        [ReadOnly] public int meshSimplificationIncrement;
        [ReadOnly] public int verticesPerLine;

        public MeshJobData meshData;

        public void Execute(int index)
        {
            float topLeftX = (borderedSize - 1) / -2f;
            float topLeftZ = (borderedSize - 1) / 2f;

            int x = index / borderedSize;
            int y = index % borderedSize;
            int triangleIndex = index * 6;

            int vertexIndex = y * borderedSize + x;
            meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve[x * borderedSize + y] * heightMultiplier, topLeftZ - y);
            meshData.uvs[vertexIndex] = new Vector2(x / (float)borderedSize, y / (float)borderedSize);

            if (x < borderedSize - 6 && y < borderedSize - 6)
            {
                meshData.AddTriangle(triangleIndex, vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                meshData.AddTriangle(triangleIndex + 3, vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
            }
        }
    }

    public struct MeshJobData
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> vertices;
        [NativeDisableParallelForRestriction]
        public NativeArray<int> triangles;
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> uvs;

        public void AddTriangle(int triangleIndex, int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
        }

        public void CreateMesh(NativeArray<Color> colourMap, ref Mesh mesh)
        {
            mesh.Clear();

            mesh.SetVertexBufferParams(vertices.Length, new VertexAttributeDescriptor(VertexAttribute.Position));
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(triangles, 0, 0, triangles.Length);

            mesh.SetUVs(0, uvs);

            mesh.subMeshCount = 1;
            var descriptor = new SubMeshDescriptor(0, triangles.Length, MeshTopology.Triangles);
            mesh.SetSubMesh(0, descriptor, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }
}
