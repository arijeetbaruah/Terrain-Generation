using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProcudualGenerator
{
    public static class Noise
    {
        public static void GenerateNoiseMap(int mapWidth, NoiseConfigData noiseConfigData, TerrainData terrainData, NativeArray<float> noiseMap)
        {
            System.Random prng = new System.Random(noiseConfigData.seed);
            NativeArray<Vector2> octaveOffsets = new NativeArray<Vector2>(noiseConfigData.octaves, Allocator.Persistent);
            for (int i = 0; i < noiseConfigData.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + noiseConfigData.offset.x;
                float offsetY = prng.Next(-100000, 100000) - noiseConfigData.offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float scale = terrainData.noiseScale;
            if (scale <= 0)
            {
                scale = 0.0001f;
            }

            NoiseJob noiseJob = new NoiseJob()
            {
                mapScale = mapWidth,
                scale = scale,
                octaves = noiseConfigData.octaves,
                persistance = noiseConfigData.persistance,
                lacunarity = noiseConfigData.lacunarity,
                noiseMap = noiseMap,
                octaveOffsets = octaveOffsets
            };

            JobHandle jobHandle = noiseJob.Schedule();
            jobHandle.Complete();

            octaveOffsets.Dispose();
        }
    }

    [BurstCompile]
    public struct NoiseJob : IJob
    {
        [ReadOnly] public int mapScale;
        [ReadOnly] public float scale;
        [ReadOnly] public int octaves;
        [ReadOnly] public float persistance;
        [ReadOnly] public float lacunarity;

        public NativeArray<float> noiseMap;
        public NativeArray<Vector2> octaveOffsets;

        public void Execute()
        {
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfScale = mapScale / 2f;

            for (int index = 0; index < mapScale * mapScale; index++)
            {
                int y = index % mapScale;
                int x = index / mapScale;

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfScale + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfScale + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[y * mapScale + x] = noiseHeight;

            }

            for (int y = 0; y < mapScale; y++)
            {
                for (int x = 0; x < mapScale; x++)
                {
                    noiseMap[y * mapScale + x] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[y * mapScale + x]);
                }
            }
        }
    }
}
