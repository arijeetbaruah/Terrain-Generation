using UnityEngine;

namespace ProcudualGenerator
{
    public class TerrainRegion : BaseMultiConfig<TerrainType, TerrainRegion>
    {
        public float savedMinHeight;
        public float savedMaxHeight;

        public void ApplyToMaterial(Material material)
        {
            UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
        }

        public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
        {
            savedMinHeight = minHeight;
            savedMaxHeight = maxHeight;

            material.SetFloat("_minHeight", minHeight);
            material.SetFloat("_maxHeight", maxHeight);
        }
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
