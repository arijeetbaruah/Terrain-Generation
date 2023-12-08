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
        public const int mapChunkSize = 239;

        [PropertyRange(0, 6)]
        public int levelOfDetail;

        [SerializeField] private ConfigRegistry configRegistry;
        [SerializeField] private Material material;

        private float[] falloffMap;
        private TerrainRegion regions;
        private TerrainConfig terrainConfig;
        private NoiseData noiseData;

        public bool autoUpdate;

        void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            GenerateMap();
        }

        [Button]
        public void GenerateMap()
        {
            if (falloffMap == null || falloffMap.Length == 0)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
            }

            if (noiseData == null && configRegistry.TryGetValue<NoiseData>(out noiseData))
            {
                return;
            }

            if (regions == null && configRegistry.TryGetValue<TerrainRegion>(out regions))
            {
                return;
            }

            if (terrainConfig == null && configRegistry.TryGetValue<TerrainConfig>(out terrainConfig))
            {
                return;
            }

            NativeArray<float> noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseData.Data, terrainConfig.Data);

            for (int x = 0; x < noiseMap.Length; x++)
            {
                if (terrainConfig.Data.useFalloff)
                {
                    noiseMap[x] = Mathf.Clamp01(noiseMap[x] - falloffMap[x]);
                }
            }

            float samples = 64;

            Gradient gradient = new Gradient();
            gradient.colorSpace = ColorSpace.Linear;
            gradient.colorKeys = regions.Data.Select(r => new GradientColorKey
            {
                time = r.height,
                color = r.colour
            }).ToArray();

            Texture2D colorMapTex = new Texture2D((int)samples, 1);
            colorMapTex.filterMode = FilterMode.Bilinear;
            colorMapTex.wrapMode = TextureWrapMode.Repeat;

            for (int i = 0; i < samples; i++)
            {
                colorMapTex.SetPixel(i, 0, gradient.Evaluate(i / samples)); // INT BY INT IS AN INT SO ITS GOING 0 FUCK MY LIFE
            }

            colorMapTex.Apply();

            regions.UpdateMeshHeights(material, colorMapTex, terrainConfig.Data.minHeight, terrainConfig.Data.maxHeight);

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
                useFalloff = terrainConfig.Data.useFalloff,
                noiseMap = noiseMap,
                falloffMap = nativeFalloffMap,
                regions = nativeTerrainType,
                colourMap = colourMap,
            };

            JobHandle jobHandle = colorMapGenerator.Schedule();
            jobHandle.Complete();


            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, colourMap, terrainConfig.Data, mapChunkSize, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));

            noiseMap.Dispose();
            colourMap.Dispose();
            nativeFalloffMap.Dispose();
            nativeTerrainType.Dispose();
        }

        void OnValidate()
        {
            configRegistry.AddListener(() =>
            {
                if (autoUpdate)
                {
                    GenerateMap();
                }
            });
            if (noiseData == null && configRegistry.TryGetValue<NoiseData>(out noiseData))
            {
                return;
            }

            if (regions == null && configRegistry.TryGetValue<TerrainRegion>(out regions))
            {
                return;
            }

            if (terrainConfig == null && configRegistry.TryGetValue<TerrainConfig>(out terrainConfig))
            {
                return;
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
