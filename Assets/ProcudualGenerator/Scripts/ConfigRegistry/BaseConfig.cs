using System;
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
        void AddListener(System.Action OnValuesUpdated);
        void RemoveListener(System.Action OnValuesUpdated);
        void RemoveAllListener();
    }

    public abstract class BaseConfig<U> : ScriptableObject, IConfig where U : IConfig
    {
        public string ID => nameof(U);

        public event System.Action OnValuesUpdated;
        public bool autoUpdate;

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
        public abstract void Initialize();

        public void AddListener(Action OnValuesUpdated)
        {
            this.OnValuesUpdated += OnValuesUpdated;
        }

        public void RemoveAllListener()
        {
            this.OnValuesUpdated = null;
        }

        public void RemoveListener(Action OnValuesUpdated)
        {
            this.OnValuesUpdated -= OnValuesUpdated;
        }
    }

    public abstract class BaseSingleConfig<T, U> : BaseConfig<U> where T : IConfigData where U : IConfig
    {
        [SerializeField] private T data;

        public T Data => data;

        public virtual void OnInitialize()
        {
        }

        public override void Initialize()
        {
            OnInitialize();
        }
    }

    public abstract class BaseMultiConfig<T, U> : BaseConfig<U> where T : IConfigData where U : IConfig
    {
        [SerializeField] private List<T> data;

        public List<T> Data => data;

        private Dictionary<string, T> dataMap;

        public virtual void OnInitialize()
        {
        }

        public override void Initialize()
        {
            if (data == null)
            {
                data = new List<T>();
            }

            dataMap = data.Where(d => !string.IsNullOrEmpty(d.ID)).ToDictionary(d => d.ID);
            OnInitialize();
        }
    }
}
