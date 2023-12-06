using UnityEngine;

namespace ProcudualGenerator
{
    public class TerrainConfig : BaseSingleConfig<TerrainData, TerrainConfig>
    {
    }

    [System.Serializable]
    public class TerrainData : IConfigData
    {
        public string ID => nameof(TerrainData);

        public float noiseScale;

        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;

        public bool useFalloff;

        public float minHeight
        {
            get
            {
                return meshHeightMultiplier * meshHeightCurve.Evaluate(0);
            }
        }

        public float maxHeight
        {
            get
            {
                return meshHeightMultiplier * meshHeightCurve.Evaluate(1);
            }
        }
    }
}
