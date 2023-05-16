///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;

using ParadoxFramework.Utilities;
using ParadoxFramework.General.Pool.ErrorHandler;

namespace ParadoxFramework.General.Pool
{
    internal class PoolData : GenericPoolData<GameObject> { }

    public struct CreationPoolArg
    {
        public string Name;
        public int Amount;
        public GameObject Prefab;
        public OptionT<Transform> Parent;
        public OptionT<Action<GameObject>> OnReturnReset;
        public OptionT<Action<GameObject>> OnFactoryCreation;

        public CreationPoolArg(string poolName, int initialAmount, GameObject prefab, Transform parent = null, Action<GameObject> onReturnPool = null, Action<GameObject> onFactoryCreation = null) : this()
        {
            Name = poolName;
            Amount = initialAmount;
            Prefab = prefab;
            Parent = new OptionT<Transform>(parent);
            OnReturnReset = new OptionT<Action<GameObject>>(onReturnPool);
        }
    }


    public class ParadoxPoolManager : MonoBehaviour
    {
        private readonly Dictionary<string, PoolData> _poolData = new();
        private static OptionT<ParadoxPoolManager> _instance;
        private static readonly object _lock = new();
        private OptionT<Transform> _trm;

        public static ParadoxPoolManager Instance
        {
            get
            {
                lock (_lock)
                    return _instance.Get(FindOrCreateDispatcher());
            }
        }


        /// <summary>
        /// Create a new pool with the given prefab.
        ///     - Check and save a reference for the transform.
        ///     - Add deactivation of the object to the OnReturnReset.
        ///     - Generate a PoolData and add it to the pools.
        ///     - Create instances.
        /// </summary>
        /// <param name="args"></param>
        public void CreatePool(CreationPoolArg args)
        {
            if (_trm.IsNull())
                _trm = new OptionT<Transform>(transform);

            var onReturn = args.OnReturnReset.GetMapped(e =>
            {
                e += i =>
                {
                    i.SetActive(false);
                    i.transform.parent = args.Parent.Get(_trm.Get());
                };
                return e;
            }, DefaultDelegates<GameObject>.EmptyCallback);

            var poolData = new PoolData()
            {
                Prefab = args.Prefab,
                Parent = args.Parent,
                OnReturnReset = onReturn,
                OnFactoryCreation = args.OnFactoryCreation.Get(delegate { }),
                AvalibleObjects = new Stack<GameObject>()
            };
            _poolData.Add(args.Name, poolData);

            for (int i = 0; i < args.Amount; i++)
                CreateInstance(args.Name, poolData);
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

            var data = _poolData[poolName];
            for (int i = 0; i < amount; i++)
                CreateInstance(poolName, data);
        }

        /// <summary>
        /// Return true if the pool exist.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public bool IsPoolExist(string poolName) => _poolData.ContainsKey(poolName);
        /// <summary>
        /// Return true if the pool is empty.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public bool IsPoolEmpty(string poolName)
        {
            PoolExistChecker(poolName, "checking if pool is empty");
            return _poolData[poolName].AvalibleObjects.Any();
        }

        /// <summary>
        /// Add another event to the return pool, called when an instance return to the pool.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="onReturnInstance"></param>
        public void AddReturnEvent(string poolName, Action<GameObject> onReturnInstance)
        {
            PoolExistChecker(poolName, "adding return event");
            _poolData[poolName].OnReturnReset += onReturnInstance;
        }
        /// <summary>
        /// Reset the return pool event, called when an instance return to the pool.
        /// </summary>
        /// <param name="poolName"></param>
        public void ResetReturnEvent(string poolName)
        {
            PoolExistChecker(poolName, "reseting return event");
            var data = _poolData[poolName];
            data.OnReturnReset = i =>
            {
                i.SetActive(false);
                i.transform.parent = data.Parent.Get(_trm.Get());
            };
        }

        /// <summary>
        /// Add another factory event, called when a new instance is created for the pool.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="onFactoryCreation"></param>
        public void AddFactoryEvent(string poolName, Action<GameObject> onFactoryCreation)
        {
            PoolExistChecker(poolName, "adding factory event");
            _poolData[poolName].OnFactoryCreation += onFactoryCreation;
        }
        /// <summary>
        /// Reset the factory event, called when a new instance is created for the pool.
        /// </summary>
        /// <param name="poolName"></param>
        public void ResetFactoryPoolEvent(string poolName)
        {
            PoolExistChecker(poolName, "reseting factory event");
            _poolData[poolName].OnFactoryCreation = delegate { };
        }

        /// <summary>
        /// Get an instance from the pool.
        ///     - Check if the pool exist.
        ///     - If the pool is empy, create a new instance and return it.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public GameObject GetInstance(string poolName)
        {
            PoolExistChecker(poolName, "get instance");

            var data = _poolData[poolName];
            if (!data.AvalibleObjects.Any())
                return CreateInstance(poolName, data, true);

            var instance = data.AvalibleObjects.Pop();
            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Return an instance to a pool.
        ///     - Check if the pool exist.
        ///     - Execute OnReturn event in that instance from the pool data.
        ///     - Return the object to the pool.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="instance"></param>
        public void ReturnInstance(string poolName, GameObject instance)
        {
            PoolExistChecker(poolName, "return instance");

            var data = _poolData[poolName];
            data.OnReturnReset(instance);
            data.AvalibleObjects.Push(instance);
        }

        /// <summary>
        /// Return an instance to a pool with a given delay.
        ///     - Check if the pool exist.
        ///     - Execute a coroutine for delaying the operation.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="instance"></param>
        /// <param name="delay"></param>
        public void ReturnInstance(string poolName, GameObject instance, float delay)
        {
            PoolExistChecker(poolName, "return instance delayed");
            StartCoroutine(DelayedReturn(_poolData[poolName], instance, delay));
        }

        /// <summary>
        /// Release all elements of the pool using a coroutine with time slicing and can remove the pool too.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="removePool"></param>
        public void DisposePool(string poolName, bool removePool = true)
        {
            PoolExistChecker(poolName, "dispose pool");

            var data = _poolData[poolName];
            StartCoroutine(TimeSlicingPoolDispose(data));

            if (removePool)
                _poolData.Remove(poolName);
        }

        /// <summary>
        /// Clean and remove all the pools, can destroy the pool manager too.
        /// </summary>
        /// <param name="destroyPoolManager"></param>
        public void DisposeAll(bool destroyPoolManager = true)
        {
            StartCoroutine(TimeSlicingDisposeAll(destroyPoolManager));
        }



        private void OnDestroy()
        {
            _instance = new OptionT<ParadoxPoolManager>();
            StartCoroutine(TimeSlicingDisposeAll(true));
        }

        private IEnumerator TimeSlicingDisposeAll(bool destroyManager)
        {
            foreach (var data in _poolData.Values)
                yield return TimeSlicingPoolDispose(data);

            _poolData.Clear();
            if (destroyManager)
                Destroy(this.gameObject);
        }

        private IEnumerator TimeSlicingPoolDispose(PoolData data)
        {
            var wait = new WaitForSeconds(0.05f);

            while (data.AvalibleObjects.Peek() != null)
            {
                Destroy(data.AvalibleObjects.Pop());
                yield return wait;
            }
        }

        private IEnumerator DelayedReturn(PoolData data, GameObject instance, float delay)
        {
            yield return new WaitForSeconds(delay);

            data.OnReturnReset(instance);
            data.AvalibleObjects.Push(instance);
        }

        private void PoolExistChecker(string poolName, string actionMessage)
        {
            if (!IsPoolExist(poolName))
                PoolErrorHandler.ThrowError(EPoolExceptions.PoolDontExistException, "ParadoxPool", actionMessage, poolName);
        }

        private GameObject CreateInstance(string name, PoolData data, bool activate = false)
        {
            var instance = GameObject.Instantiate(data.Prefab, data.Parent.Get(_trm.Get()));
            instance.SetActive(activate);
            data.AvalibleObjects.Push(instance);
            data.OnFactoryCreation(instance);
            return instance;
        }


        private static ParadoxPoolManager FindOrCreateDispatcher()
        {
            var instance = GameObject.FindObjectOfType<ParadoxPoolManager>();
            if (instance == null)
                instance = new GameObject("--ParadoxPoolDispatcher--").AddComponent<ParadoxPoolManager>();

            _instance = new OptionT<ParadoxPoolManager>(instance);
            return instance;
        }
    }
}
