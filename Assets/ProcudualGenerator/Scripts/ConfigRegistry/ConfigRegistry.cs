using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ProcudualGenerator
{
    [CreateAssetMenu]
    public class ConfigRegistry : ScriptableObject
    {
        [SerializeField, FolderPath] private string configPath;

        [OdinSerialize] public IConfig[] config;
    }
}
