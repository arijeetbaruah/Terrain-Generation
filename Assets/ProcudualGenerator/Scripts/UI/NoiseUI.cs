using ProcudualGenerator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField octaves;
    [SerializeField] private Slider persistance;
    [SerializeField] private TMP_InputField lacunarity;
    [SerializeField] private TMP_InputField seed;
    [SerializeField] private TMP_InputField offsetX;
    [SerializeField] private TMP_InputField offsetY;

    [SerializeField] private NoiseData data;
    [SerializeField] private MapGenerator mapGenerator;

    private void Start()
    {
        octaves.text = data.Data.octaves.ToString();
        lacunarity.text = data.Data.lacunarity.ToString();
        seed.text = data.Data.seed.ToString();
        persistance.value = data.Data.persistance;
        offsetX.text = data.Data.offset.x.ToString();
        offsetY.text = data.Data.offset.y.ToString();
    }

    private void OnEnable()
    {
        octaves.onValueChanged.AddListener(val =>
        {
            data.Data.octaves = int.Parse(val.ToString());
            mapGenerator.GenerateMap();
        });

        lacunarity.onValueChanged.AddListener(val =>
        {
            data.Data.lacunarity = int.Parse(val.ToString());
            mapGenerator.GenerateMap();
        });

        seed.onValueChanged.AddListener(val =>
        {
            data.Data.seed = int.Parse(val.ToString());
            mapGenerator.GenerateMap();
        });

        persistance.onValueChanged.AddListener(val =>
        {
            data.Data.persistance = val;
            mapGenerator.GenerateMap();
        });

        offsetX.onValueChanged.AddListener(val =>
        {
            data.Data.persistance = float.Parse(val);
            mapGenerator.GenerateMap();
        });

        offsetY.onValueChanged.AddListener(val =>
        {
            data.Data.persistance = float.Parse(val);
            mapGenerator.GenerateMap();
        });
    }
}
