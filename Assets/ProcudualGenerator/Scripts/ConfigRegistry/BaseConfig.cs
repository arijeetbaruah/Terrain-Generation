using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProcudualGenerator
{
    public interface IConfigData
    {
        string ID { get; }
    }

    public interface IConfig
    {
        string ID { get; }
        void Initialize();
    }

    public abstract class BaseConfig<U> : ScriptableObject, IConfig where U : IConfig
    {
        public string ID => nameof(U);

        public abstract void Initialize();
    }

    public abstract class BaseSingleConfig<T, U> : BaseConfig<U> where T : IConfigData where U : IConfig
    {
        [SerializeField] private T data;

        public event System.Action OnValuesUpdated;
        public bool autoUpdate;

        public T Data => data;

        public virtual void OnInitialize()
        {
        }

        public override void Initialize()
        {
            OnInitialize();
        }

        protected virtual void OnValidate()
        {
            if (autoUpdate)
            {
                NotifyOfUpdatedValues();
                Initialize();
            }
        }

        public void NotifyOfUpdatedValues()
        {
            if (OnValuesUpdated != null)
            {
                OnValuesUpdated();
            }
        }
    }

    public abstract class BaseMultiConfig<T, U> : BaseConfig<U> where T : IConfigData where U : IConfig
    {
        [SerializeField] private T[] data;

        public event System.Action OnValuesUpdated;
        public bool autoUpdate;

        public T[] Data => data;

        private Dictionary<string, T> dataMap;

        public virtual void OnInitialize()
        {
        }

        public override void Initialize()
        {
            dataMap = data.ToDictionary(d => d.ID);
            OnInitialize();
        }

        protected virtual void OnValidate()
        {
            if (autoUpdate)
            {
                NotifyOfUpdatedValues();
                Initialize();
            }
        }

        public void NotifyOfUpdatedValues()
        {
            if (OnValuesUpdated != null)
            {
                OnValuesUpdated();
            }
        }
    }
}
