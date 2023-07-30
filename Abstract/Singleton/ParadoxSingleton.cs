using UnityEngine;
using System;

using ParadoxFramework.Utilities;

namespace ParadoxFramework.Abstract
{
    public abstract class ParadoxSingleton<T> : MonoBehaviour, IDisposable where T : MonoBehaviour
    {
        private static OptionT<T> _instance;
        private static readonly object _lock = new();
        /// <summary>
        /// Determinate if the singleton persist between scenes.
        /// </summary>
        protected abstract bool IsPersistant { get; }

        public static T Instance
        {
            get
            {
                lock(_lock)
                    return _instance.Get(FindOrCreateInstance());
            }
        }
        public static bool IsNull => _instance.IsNull();

        protected virtual void Awake()
        {
            if (IsPersistant)
                DontDestroyOnLoad(this.gameObject);
        }

        public void Dispose()
        {
            _instance = new OptionT<T>();
            Destroy(this.gameObject);
        }

        private static T FindOrCreateInstance()
        {
            string tagName = $"{typeof(T).Name}Singleton";
            var obj = GameObject.FindGameObjectWithTag(tagName).GetComponent<T>();
            _instance = new OptionT<T>(obj, CreateNewInstance(tagName));

            return _instance.Get();
        }

        private static T CreateNewInstance(string tagName)
        {
            var obj = new GameObject($"--{tagName}--").AddComponent<T>();
            obj.tag = tagName;
            return obj;
        }
    }
}