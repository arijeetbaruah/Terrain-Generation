using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProcudualGenerator
{
    public static class Noise
    {
        public static void GenerateNoiseMap(int mapScale, NoiseConfigData noiseConfigData, TerrainData terrainData, NativeArray<float> noiseMap)
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
                mapScale = mapScale,
                scale = scale,
                octaves = noiseConfigData.octaves,
                persistance = noiseConfigData.persistance,
                lacunarity = noiseConfigData.lacunarity,
                noiseMap = noiseMap,
                octaveOffsets = octaveOffsets
            };

            JobHandle jobHandle = noiseJob.Schedule(mapScale * mapScale, mapScale);
            jobHandle.Complete();

            float minNoiseHeight = noiseJob.noiseMap.Min();
            float maxNoiseHeight = noiseJob.noiseMap.Max();

            NoiseLerper noiseLerper = new NoiseLerper()
            {
                noiseMap = noiseJob.noiseMap,
                maxNoiseHeight = maxNoiseHeight,
                minNoiseHeight = minNoiseHeight
            };

            noiseLerper.Schedule(mapScale * mapScale, mapScale).Complete();

            octaveOffsets.Dispose();
        }
    }

    [BurstCompile]
    public struct NoiseLerper : IJobParallelFor
    {
        [ReadOnly] public float minNoiseHeight;
        [ReadOnly] public float maxNoiseHeight;

        [NativeDisableParallelForRestriction]
        public NativeArray<float> noiseMap;

        public void Execute(int index)
        {
            noiseMap[index] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[index]);
        }
    }

    [BurstCompile]
    public struct NoiseJob : IJobParallelFor
    {
        [ReadOnly] public int mapScale;
        [ReadOnly] public float scale;
        [ReadOnly] public int octaves;
        [ReadOnly] public float persistance;
        [ReadOnly] public float lacunarity;

        [NativeDisableParallelForRestriction]
        public NativeArray<float> noiseMap;
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> octaveOffsets;

        public void Execute(int index)
        {
            float halfScale = mapScale / 2f;

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

            noiseMap[index] = noiseHeight;
        }

    }
}
