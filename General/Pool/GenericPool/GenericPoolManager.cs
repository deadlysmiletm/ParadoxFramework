using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ParadoxFramework.Utilities;
using ParadoxFramework.General.Pool.ErrorHandler;

namespace ParadoxFramework.General.Pool
{
    internal class PoolObjectPoolData : GenericPoolData<IPoolObject> { }

    public struct CreationGenericPoolArg
    {
        public string Name;
        public int Amount;
        public IPoolObject Obj;

        public CreationGenericPoolArg(string poolName, int initialAmount, IPoolObject obj)
        {
            Name = poolName;
            Amount = initialAmount;
            Obj = obj;
        }
    }


    public class GenericPool : IEnumerable<IPoolObject>
    {
        private readonly GenericPoolData<IPoolObject> _poolData = new();


        public GenericPool(IPoolObject objPrefab, int amount)
        {
            _poolData.Prefab = objPrefab;
            _poolData.AvalibleObjects = new Stack<IPoolObject>();

            for (int i = 0; i < amount; i++)
                CreateInstanceWithoutReturn();
        }

        /// <summary>
        /// Return true if the pool is empty.
        /// </summary>
        /// <returns></returns>
        public bool IsPoolEmpty() => _poolData.AvalibleObjects.Count == 0;

        /// <summary>
        /// Return the actual amount of instances on the pool.
        /// </summary>
        /// <returns></returns>
        public int PoolCount() => _poolData.AvalibleObjects.Count;

        /// <summary>
        /// Fill a pool with the given amount of elements.
        /// </summary>
        /// <param name="amount"></param>
        public void FillPool(int amount)
        {
            for (int i = 0; i < amount; i++)
                CreateInstanceWithoutReturn();
        }

        /// <summary>
        /// Get an instance from the pool, how the creation is async need a callback to set the object.
        ///     - If the pool is empy, create a new instance.
        ///     - Excecute the callback with the instance.
        /// </summary>
        /// <returns></returns>
        public IPoolObject GetInstance()
        {
            if (_poolData.AvalibleObjects.Count == 0)
                return CreateInstance();

            return _poolData.AvalibleObjects.Pop();
        }

        /// <summary>
        /// Get an instance from the pool, but don't create dinamically a new instance if the pool is empty.
        /// </summary>
        /// <returns></returns>
        public OptionT<IPoolObject> GetInstanceUnsafe()
        {
            if (_poolData.AvalibleObjects.Count == 0)
                return new OptionT<IPoolObject>();

            return new OptionT<IPoolObject>(_poolData.AvalibleObjects.Pop());
        }

        /// <summary>
        /// Return an instance to the pool.
        /// </summary>
        /// <param name="instance"></param>
        public void ReturnInstance(IPoolObject instance)
        {
            _poolData.Prefab.OnPoolReturn(instance);
            _poolData.AvalibleObjects.Push(instance);
        }

        /// <summary>
        /// Release all elements of the pool from memory.
        /// </summary>
        public void DisposePool()
        {
            while (_poolData.AvalibleObjects.Count > 0)
                _poolData.AvalibleObjects.Pop().Dispose();
        }

        /// <summary>
        /// Return a coroutine to release all elements of the pool using time slicing. Recommended for large pools to avoid application freezing.
        /// </summary>
        /// <returns></returns>
        public IEnumerator TimeSlicingDispose()
        {
            var wait = new WaitForSeconds(0.05f);
            while (_poolData.AvalibleObjects.Count > 0)
            {
                _poolData.AvalibleObjects.Pop().Dispose();
                yield return wait;
            }
        }


        private IPoolObject CreateInstance()
        {
            var instance = _poolData.Prefab.FactoryMethod();
            _poolData.AvalibleObjects.Push(instance);
            return instance;
        }
        private void CreateInstanceWithoutReturn()
        {
            var instance = _poolData.Prefab.FactoryMethod();
            _poolData.AvalibleObjects.Push(instance);
        }

        public IEnumerator<IPoolObject> GetEnumerator()
        {
            foreach (var poolObject in _poolData.AvalibleObjects)
                yield return poolObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }
    }

    public class GenericPoolManager : MonoBehaviour
    {
        private readonly Dictionary<string, GenericPool> _poolData = new();
        private static OptionT<GenericPoolManager> _instance;
        private static readonly object _lock = new();
        private OptionT<Transform> _trm;

        public static GenericPoolManager Instance
        {
            get
            {
                lock (_lock)
                    return _instance.Get(FindOrCreateDispatcher());
            }
        }

        /// <summary>
        /// Create a new GenericPool with the given IPoolObject and initial amount.
        /// </summary>
        /// <param name="arg"></param>
        public void CreatePool(CreationGenericPoolArg arg)
        {
            if (_trm.IsNull())
                _trm = new OptionT<Transform>(transform);

            var pool = new GenericPool(arg.Obj, arg.Amount);
            _poolData.Add(arg.Name, pool);
            pool.FillPool(arg.Amount);
        }

        /// <summary>
        /// Fill a pool with the given amount of elements.
        ///     - Check if the pool exist.
        ///     - Create the instances.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="amount"></param>
        public void FillPool(string poolName, int amount)
        {
            PoolExistChecker(poolName, "fill pool");
            _poolData[poolName].FillPool(amount);
        }

        /// <summary>
        /// Return true if the pool exist.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public bool IsPoolExist(string poolName) => _poolData.ContainsKey(poolName);

        /// <summary>
        /// Return the actual amount of instances on the pool.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public int PoolCount(string poolName)
        {
            PoolExistChecker(poolName, "pool count");
            return _poolData[poolName].PoolCount();
        }

        /// <summary>
        /// Return true if the pool is empty.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public bool IsPoolEmpty(string poolName)
        {
            PoolExistChecker(poolName, "checking if pool is empty");
            return _poolData[poolName].IsPoolEmpty();
        }

        public IPoolObject GetPoolObject(string poolName)
        {
            PoolExistChecker(poolName, "get instance");
            return _poolData[poolName].GetInstance();
        }

        public void ReturnInstance(string poolName, IPoolObject obj)
        {
            PoolExistChecker(poolName, "return instance");

            _poolData[poolName].ReturnInstance(obj);
        }

        public void DisposePool(string poolName, bool removePool = true)
        {
            PoolExistChecker(poolName, "dispose Time slicing pool");
            StartCoroutine(_poolData[poolName].TimeSlicingDispose());

            if (removePool)
                _poolData.Remove(poolName);
        }

        public void DisposeAll(bool destroyPoolManager = true)
        {
            StartCoroutine(TimeSlicingDispose(destroyPoolManager));
        }

        private IEnumerator TimeSlicingDispose(bool destroyPoolManager)
        {
            foreach (var pool in _poolData)
                yield return pool.Value.TimeSlicingDispose();

            _poolData.Clear();
            if (destroyPoolManager)
                Destroy(this.gameObject);
        }


        private void PoolExistChecker(string poolName, string actionMessage)
        {
            if (!IsPoolExist(poolName))
                PoolErrorHandler.ThrowError(EPoolExceptions.PoolDontExistException, "ParadoxPool", actionMessage, poolName);
        }

        private static GenericPoolManager FindOrCreateDispatcher()
        {
            _instance = new OptionT<GenericPoolManager>(GameObject.FindGameObjectWithTag("GPoolManager").GetComponent<GenericPoolManager>(), CreateInstance());
            return _instance.Get();
        }

        private static GenericPoolManager CreateInstance()
        {
            var instance = new GameObject("--GenericPoolDispatcher--").AddComponent<GenericPoolManager>();
            instance.tag = "GPoolManager";
            return instance;
        }
    }
}