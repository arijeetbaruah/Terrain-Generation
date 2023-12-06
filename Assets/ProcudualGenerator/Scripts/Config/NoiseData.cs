using Sirenix.OdinInspector;
using UnityEngine;

namespace ProcudualGenerator
{
    public class NoiseData : BaseSingleConfig<NoiseConfigData, TerrainRegion>
    {
        protected override void OnValidate()
        {
            base.OnValidate();

            if (Data.lacunarity < 1)
            {
                Data.lacunarity = 1;
            }
            if (Data.octaves < 0)
            {
                Data.octaves = 0;
            }
        }
    }

    [System.Serializable]
    public class NoiseConfigData : IConfigData
    {
        public string ID => nameof(NoiseConfigData);

        public int octaves;
        [PropertyRange(0, 1)]
        public float persistance;
        public float lacunarity;

        public int seed;
        public Vector2 offset;
    }
}
