using ProcudualGenerator;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeshGeneratorUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField noiseScale;
    [SerializeField] private TMP_InputField heightMultiplier;
    [SerializeField] private Toggle useFallout;
    [SerializeField] private TMP_Dropdown terrainTypeDropdown;

    [SerializeField] private TerrainConfig data;
    [SerializeField] private NoiseData noiseData;
    [SerializeField] private MapGenerator mapGenerator;

    private void Awake()
    {
        noiseScale.text = data.Data.noiseScale.ToString();
        heightMultiplier.text = data.Data.meshHeightMultiplier.ToString();
        useFallout.isOn = data.Data.useFalloff;

        var options = Enum.GetNames(typeof(TextureType)).ToList();
        terrainTypeDropdown.ClearOptions();
        terrainTypeDropdown.AddOptions(options);
        terrainTypeDropdown.value = options.IndexOf(noiseData.Data.textureType.ToString());
    }

    private void OnEnable()
    {
        noiseScale.onValueChanged.AddListener(val =>
        {
            data.Data.noiseScale = float.Parse(val.ToString());
            mapGenerator.GenerateMap();
        });

        heightMultiplier.onValueChanged.AddListener(val =>
        {
            data.Data.meshHeightMultiplier = float.Parse(val.ToString());
            mapGenerator.GenerateMap();
        });

        useFallout.onValueChanged.AddListener(val =>
        {
            data.Data.useFalloff = val;
            mapGenerator.GenerateMap();
        });

        terrainTypeDropdown.onValueChanged.AddListener(val =>
        {
            noiseData.Data.textureType = (TextureType)val;
            mapGenerator.GenerateMap();
        });
    }
}
