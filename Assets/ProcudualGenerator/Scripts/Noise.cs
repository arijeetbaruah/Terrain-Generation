using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProcudualGenerator
{
    public static class Noise
    {
        public static void GenerateNoiseMap(int mapWidth, int mapHeight, NoiseConfigData noiseConfigData, TerrainData terrainData, NativeArray<float> noiseMap)
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
                mapWidth = mapWidth,
                mapHeight = mapHeight,
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
        [ReadOnly] public int mapWidth;
        [ReadOnly] public int mapHeight;
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

            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {

                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

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
                    noiseMap[y * mapWidth + x] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[y * mapWidth + x] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[y * mapWidth + x]);
                }
            }
        }
    }
}
