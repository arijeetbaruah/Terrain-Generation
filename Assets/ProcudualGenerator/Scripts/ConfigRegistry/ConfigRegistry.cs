using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProcudualGenerator
{
    [CreateAssetMenu]
    public class ConfigRegistry : SerializedScriptableObject
    {
        [SerializeField, FolderPath] private string configPath;

        [ShowInInspector] public Dictionary<string, IConfig> config;

        public event System.Action OnValuesUpdated;

        public bool TryGetValue<T>(out T value) where T : IConfig
        {
            bool ret = config.TryGetValue(typeof(T).Name, out IConfig c);

            value = (T)c;

            return ret;
        }

        public void AddListener(Action callback)
        {
            OnValuesUpdated = callback;

            foreach (var c in config)
            {
                c.Value.RemoveAllListener();
                c.Value.AddListener(OnValuesUpdated);
            }
        }

        public void RemoveAllListener()
        {
            OnValuesUpdated = null;
        }

        public void OnValidate()
        {
            foreach (var c in config)
            {
                c.Value.RemoveAllListener();
                c.Value.AddListener(OnValuesUpdated);
            }
        }

#if UNITY_EDITOR
        [Button]
        private void Generate()
        {
            Type type = typeof(IConfig);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies.SelectMany(asm => asm.GetTypes())
                .Where(asm => type.IsAssignableFrom(asm) && !asm.IsAbstract && !asm.IsInterface);

            foreach (var t in types)
            {
                if (!config.ContainsKey(t.Name))
                {
                    ScriptableObject so = ScriptableObject.CreateInstance(t);
                    config.Add(t.Name, (IConfig)so);

                    UnityEditor.AssetDatabase.CreateAsset(so, $"{configPath}/{t.Name}.asset");
                }
            }
        }
#endif
    }
}
