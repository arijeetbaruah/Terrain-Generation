using Sirenix.OdinInspector;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProcudualGenerator
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColourMap, Mesh };
        public DrawMode drawMode;

        public int mapChunkSize
        {
            get
            {
                return 239;
            }
        }

        [PropertyRange(0, 6)]
        public int levelOfDetail;
        public float noiseScale;

        public int octaves;
        [PropertyRange(0, 1)]
        public float persistance;
        public float lacunarity;

        public int seed;
        public Vector2 offset;

        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;

        public bool useFalloff;
        public bool autoUpdate;

        public TerrainRegion regions;

        private float[] falloffMap;

        void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            GenerateMap();
        }

        [Button]
        public void GenerateMap()
        {
            if (falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            }

            NativeArray<float> noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
            NativeArray<Color> colourMap = new NativeArray<Color>(mapChunkSize * mapChunkSize, Allocator.TempJob);

            NativeArray<float> nativeFalloffMap = new NativeArray<float>(falloffMap, Allocator.TempJob);
            NativeArray<NativeTerrainType> nativeTerrainType = new NativeArray<NativeTerrainType>(regions.Data.Select(region => new NativeTerrainType()
            {
                colour = region.colour,
                height = region.height
            }).ToArray(), Allocator.TempJob);
            ColorMapGenerator colorMapGenerator = new ColorMapGenerator()
            {
                mapChunkSize = mapChunkSize,
                useFalloff = useFalloff,
                noiseMap = noiseMap,
                falloffMap = nativeFalloffMap,
                regions = nativeTerrainType,
                colourMap = colourMap,
            };

            JobHandle jobHandle = colorMapGenerator.Schedule();
            jobHandle.Complete();


            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, colourMap, meshHeightMultiplier, mapChunkSize, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));

            noiseMap.Dispose();
            colourMap.Dispose();
            nativeFalloffMap.Dispose();
            nativeTerrainType.Dispose();
        }

        void OnValidate()
        {
            if (lacunarity < 1)
            {
                lacunarity = 1;
            }
            if (octaves < 0)
            {
                octaves = 0;
            }

            if (autoUpdate)
            {
                GenerateMap();
            }

            falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        }
    }

    [BurstCompile]
    public struct ColorMapGenerator : IJob
    {
        [Unity.Collections.ReadOnly] public int mapChunkSize;
        [Unity.Collections.ReadOnly] public bool useFalloff;
        [Unity.Collections.ReadOnly] public NativeArray<float> falloffMap;
        [Unity.Collections.ReadOnly] public NativeArray<NativeTerrainType> regions;

        public NativeArray<float> noiseMap;
        public NativeArray<Color> colourMap;

        public void Execute()
        {
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if (useFalloff)
                    {
                        noiseMap[y * mapChunkSize + x] = Mathf.Clamp01(noiseMap[y * mapChunkSize + x] - falloffMap[y * mapChunkSize + x]);
                    }
                    float currentHeight = noiseMap[y * mapChunkSize + x];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colourMap[y * mapChunkSize + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }
        }
    }

    public struct NativeTerrainType
    {
        public float height;
        public Color colour;
    }
}
