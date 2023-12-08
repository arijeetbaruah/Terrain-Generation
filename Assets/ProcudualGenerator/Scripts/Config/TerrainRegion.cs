using UnityEngine;

namespace ProcudualGenerator
{
    public class TerrainRegion : BaseMultiConfig<TerrainType, TerrainRegion>
    {
        public float savedMinHeight;
        public float savedMaxHeight;
        public Gradient terrainGradiant;

        public void ApplyToMaterial(Texture2D colorMap, Material material)
        {
            UpdateMeshHeights(material, colorMap, savedMinHeight, savedMaxHeight);
        }

        public void UpdateMeshHeights(Material material, Texture2D colorMap, float minHeight, float maxHeight)
        {
            savedMinHeight = minHeight;
            savedMaxHeight = maxHeight;

            material.SetFloat("_minHeight", minHeight);
            material.SetFloat("_maxHeight", maxHeight);
            material.SetTexture("_colorMap", colorMap);
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
