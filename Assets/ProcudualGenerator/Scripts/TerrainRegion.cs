using UnityEngine;

namespace ProcudualGenerator
{
    public class TerrainRegion : BaseMultiConfig<TerrainType, TerrainRegion>
    {
    }

    [System.Serializable]
    public class TerrainType : IConfigData
    {
        public string name;
        public float height;
        public Color colour;

        public string ID => name;
    }
}
