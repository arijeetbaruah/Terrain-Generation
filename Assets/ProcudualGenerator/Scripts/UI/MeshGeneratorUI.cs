using ProcudualGenerator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeshGeneratorUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField noiseScale;
    [SerializeField] private TMP_InputField heightMultiplier;
    [SerializeField] private Toggle useFallout;

    [SerializeField] private TerrainConfig data;
    [SerializeField] private MapGenerator mapGenerator;

    private void Awake()
    {
        noiseScale.text = data.Data.noiseScale.ToString();
        heightMultiplier.text = data.Data.meshHeightMultiplier.ToString();
        useFallout.isOn = data.Data.useFalloff;
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
    }
}
