using Unity.Collections;
using UnityEngine;

namespace ProcudualGenerator
{
    public static class TextureGenerator
    {
        public static Texture2D TextureFromColourMap(NativeArray<Color> colourMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colourMap.ToArray());
            texture.Apply();
            return texture;
        }


        public static Texture2D TextureFromHeightMap(NativeArray<float> heightMap, int width, int height)
        {
            NativeArray<Color> colourMap = new NativeArray<Color>(width * height, Allocator.Persistent);
            for (int y = 0; y < height * width; y++)
            {
                colourMap[y] = Color.Lerp(Color.black, Color.white, heightMap[y]);
            }

            return TextureFromColourMap(colourMap, width, height);
        }
    }
}
