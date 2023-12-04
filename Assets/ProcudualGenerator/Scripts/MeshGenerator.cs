using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProcudualGenerator
{
    public static class MeshGenerator
    {
        public static Mesh GenerateTerrainMesh(NativeArray<float> heightMap, float heightMultiplier, int borderedSize, AnimationCurve heightCurve, int levelOfDetail, bool useFlatShading)
        {
            int meshSize = borderedSize - 2;
            int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
            int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

            NativeArray<Vector3> vertices = new NativeArray<Vector3>(verticesPerLine * verticesPerLine, Allocator.Persistent);
            NativeArray<Vector2> uvs = new NativeArray<Vector2>(verticesPerLine * verticesPerLine, Allocator.Persistent);
            NativeArray<int> triangles = new NativeArray<int>((verticesPerLine - 1) * (verticesPerLine - 1) * 6, Allocator.Persistent);

            NativeArray<int> borderTriangle = new NativeArray<int>(24 * verticesPerLine, Allocator.Persistent);
            NativeArray<Vector3> borderVertices = new NativeArray<Vector3>(verticesPerLine * 4 + 4, Allocator.Persistent);

            NativeArray<Vector2> flatShadingUVS = new NativeArray<Vector2>(vertices.Length, Allocator.Persistent);
            NativeArray<Vector3> flatShadingTriangles = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);
            NativeArray<Vector3> bakedNormals = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);

            MeshJobData meshJobData = new MeshJobData()
            {
                triangles = triangles,
                vertices = vertices,
                uvs = uvs,

                bakedNormals = bakedNormals,

                borderTriangle = borderTriangle,
                borderVertices = borderVertices,

                flatShadingUVs = flatShadingUVS,
                flatShadingVertice = flatShadingTriangles,
                useFlatShading = useFlatShading
            };

            NativeArray<float> heightCurveMap = new NativeArray<float>(borderedSize * borderedSize, Allocator.Persistent);

            for (int i = 0; i < borderedSize * borderedSize; i++)
            {
                heightCurveMap[i] = heightCurve.Evaluate(heightMap[i]);
            }

            NativeArray<int> vertexIndexesMap = new NativeArray<int>(borderedSize * borderedSize, Allocator.Persistent);

            MeshGeneratorJob meshGeneratorJob = new MeshGeneratorJob()
            {
                heightMap = heightMap,
                heightMultiplier = heightMultiplier,
                borderedSize = borderedSize,
                meshSize = meshSize,
                heightCurve = heightCurveMap,
                levelOfDetail = levelOfDetail,
                meshSimplificationIncrement = meshSimplificationIncrement,
                verticesPerLine = verticesPerLine,
                meshData = meshJobData,
                vertexIndexesMap = vertexIndexesMap,
            };

            JobHandle jobHandle = meshGeneratorJob.Schedule();

            jobHandle.Complete();
            Mesh mesh = meshGeneratorJob.meshData.CreateMesh();

            vertices.Dispose();
            uvs.Dispose();
            triangles.Dispose();
            borderTriangle.Dispose();
            borderVertices.Dispose();
            heightCurveMap.Dispose();
            flatShadingUVS.Dispose();
            flatShadingTriangles.Dispose();
            bakedNormals.Dispose();

            return mesh;
        }
    }

    [BurstCompile]
    public struct MeshGeneratorJob : IJob
    {
        [ReadOnly] public NativeArray<float> heightMap;
        [ReadOnly] public float heightMultiplier;
        [ReadOnly] public int borderedSize;
        [ReadOnly] public int meshSize;
        [ReadOnly] public NativeArray<float> heightCurve;
        [ReadOnly] public int levelOfDetail;
        [ReadOnly] public int meshSimplificationIncrement;
        [ReadOnly] public int verticesPerLine;

        public MeshJobData meshData;
        public NativeArray<int> vertexIndexesMap;

        public void Execute()
        {
            int meshSizeUnsimplified = borderedSize - 2;

            float topLeftX = (meshSizeUnsimplified - 1) / -2f;
            float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

            int meshVertexIndex = 0;
            int borderVertexIndex = -1;

            for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
                {
                    bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                    if (isBorderVertex)
                    {
                        vertexIndexesMap[x * borderedSize + y] = borderVertexIndex;
                        borderVertexIndex--;
                    }
                    else
                    {
                        vertexIndexesMap[x * borderedSize + y] = meshVertexIndex;
                        meshVertexIndex++;
                    }
                }
            }

            for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
                {
                    int vertexIndex = GetVertexIndex(x, y);
                    Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                    float height = heightCurve[y * borderedSize + x] * heightMultiplier;
                    Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                    meshData.AddVertex(vertexPosition, percent, vertexIndex);

                    if (x < borderedSize - meshSimplificationIncrement && y < borderedSize - meshSimplificationIncrement)
                    {
                        int a = GetVertexIndex(x, y);
                        int b = GetVertexIndex(x + meshSimplificationIncrement, y);
                        int c = GetVertexIndex(x, y + meshSimplificationIncrement);
                        int d = GetVertexIndex(x + meshSimplificationIncrement, y + meshSimplificationIncrement);
                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }

                    vertexIndex++;
                }
            }

            meshData.FinalizeNormals();
        }

        private int GetVertexIndex(int x, int y)
        {
            return vertexIndexesMap[x * borderedSize + y];
        }
    }

    public struct MeshJobData
    {
        public NativeArray<Vector3> vertices;
        public NativeArray<int> triangles;
        public NativeArray<Vector2> uvs;

        public NativeArray<Vector3> borderVertices;
        public NativeArray<int> borderTriangle;

        private int triangleIndex;
        private int borderTriangleIndex;

        public NativeArray<Vector3> flatShadingVertice;
        public NativeArray<Vector2> flatShadingUVs;
        public NativeArray<Vector3> bakedNormals;
        public bool useFlatShading;

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0)
            {
                borderVertices[-vertexIndex - 1] = vertexPosition;
            }
            else
            {
                vertices[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = uv;
            }
        }

        public void AddTriangle(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
            {
                borderTriangle[borderTriangleIndex] = a;
                borderTriangle[borderTriangleIndex + 1] = b;
                borderTriangle[borderTriangleIndex + 2] = c;
                borderTriangleIndex += 3;
            }
            else
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }

        }

        public void BakeNormals()
        {
            int triangelCount = triangles.Length / 3;

            for (int i = 0; i < triangelCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndex(vertexIndexA, vertexIndexB, vertexIndexC);
                bakedNormals[vertexIndexA] += triangleNormal;
                bakedNormals[vertexIndexB] += triangleNormal;
                bakedNormals[vertexIndexC] += triangleNormal;
            }

            int borderTriangelCount = borderTriangle.Length / 3;
            for (int i = 0; i < borderTriangelCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = borderTriangle[normalTriangleIndex];
                int vertexIndexB = borderTriangle[normalTriangleIndex + 1];
                int vertexIndexC = borderTriangle[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndex(vertexIndexA, vertexIndexB, vertexIndexC);
                if (vertexIndexA >= 0)
                {
                    bakedNormals[vertexIndexA] += triangleNormal;
                }
                if (vertexIndexB >= 0)
                {
                    bakedNormals[vertexIndexB] += triangleNormal;
                }
                if (vertexIndexC >= 0)
                {
                    bakedNormals[vertexIndexC] += triangleNormal;
                }
            }

            for (int i = 0; i < bakedNormals.Length; i++)
            {
                bakedNormals[i].Normalize();
            }
        }

        public Vector3 SurfaceNormalFromIndex(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
            Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
            Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        public void FinalizeNormals()
        {
            if (useFlatShading)
            {
                FlatShading();
            }
            else
            {
                BakeNormals();
            }
        }

        public void FlatShading()
        {
            for (int i = 0; i < flatShadingVertice.Length; i++)
            {
                flatShadingVertice[i] = vertices[triangles[i]];
                flatShadingUVs[i] = uvs[triangles[i]];
                triangles[i] = i;
            }

            vertices = flatShadingVertice;
            uvs = flatShadingUVs;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            if (useFlatShading)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                mesh.normals = bakedNormals.ToArray();
            }
            return mesh;
        }
    }
}
