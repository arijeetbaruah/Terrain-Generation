namespace ProcudualGenerator
{
    public class NoiseData : BaseSingleConfig<NoiseConfigData, TerrainRegion>
    {

    }

    public class NoiseConfigData : IConfigData
    {
        public string ID => nameof(NoiseConfigData);
    }
}
