using UnityEngine;

namespace ProcudualGenerator
{
    public struct TextureData
    {
        public Texture2D[] baseTexturesMap;
        public Texture2D[] normalTexturesMap;
        public Texture2D[] occlusionMap;
        public Texture2D[] glossMap;
    }

    public class TerrainRegion : BaseMultiConfig<TerrainType, TerrainRegion>
    {
        public float savedMinHeight;
        public float savedMaxHeight;

        public void UpdateMeshHeights(Material material, Texture2D colorMap, TextureData textureData, float minHeight, float maxHeight)
        {
            savedMinHeight = minHeight;
            savedMaxHeight = maxHeight;
            Texture2DArray texture2DArray = GenerateTexture2DArray(textureData.baseTexturesMap);
            Texture2DArray normal2DArray = GenerateTexture2DArray(textureData.normalTexturesMap);
            Texture2DArray occlusionMapArray = GenerateTexture2DArray(textureData.occlusionMap);
            Texture2DArray glossMapArray = GenerateTexture2DArray(textureData.glossMap);

            material.SetFloat("_minHeight", minHeight);
            material.SetFloat("_maxHeight", maxHeight);
            material.SetTexture("_colorMap", colorMap);
            material.SetTexture("_baseTextureMap", texture2DArray);
            material.SetTexture("_normalTextiureMap", normal2DArray);
            material.SetTexture("_occlusionMap", occlusionMapArray);
            material.SetTexture("_glossMap", occlusionMapArray);
        }

        private Texture2DArray GenerateTexture2DArray(Texture2D[] baseTexturesMap)
        {
            Texture2DArray texture2DArray = new Texture2DArray(baseTexturesMap[0].width, baseTexturesMap[0].height, baseTexturesMap.Length, TextureFormat.RGBA32, true, false);
            texture2DArray.filterMode = FilterMode.Bilinear;
            texture2DArray.wrapMode = TextureWrapMode.Repeat;

            for (int index = 0; index < baseTexturesMap.Length; index++)
            {
                Color[] color = baseTexturesMap[index].GetPixels();
                texture2DArray.SetPixels(color, index, 0);
            }
            texture2DArray.Apply();
            return texture2DArray;
        }
    }

    [System.Serializable]
    public class TerrainType : IConfigData
    {
        public string name;
        public float height;
        public Color colour;
        public Texture2D texture;
        public Texture2D normal;
        public Texture2D occlusion;
        public Texture2D gloss;

        public string ID => name;
    }
}
