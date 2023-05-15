///	� Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using ParadoxFramework.Utilities;
using ParadoxFramework.General.Pool.ErrorHandler;

namespace ParadoxFramework.General.Pool
{
    internal class AddressPoolData : GenericPoolData<AssetReferenceGameObject> { }

    public struct AddressCreationPoolArg
    {
        public string Name;
        public int Amount;
        public AssetReferenceGameObject Reference;
        public OptionT<Transform> Parent;
        public OptionT<Action<GameObject>> OnReturnReset;

        public AddressCreationPoolArg(string poolName, int initialAmount, AssetReferenceGameObject prefabReference, Transform parent = null, Action<GameObject> onReturnPool = null) : this()
        {
            Name = poolName;
            Amount = initialAmount;
            Reference = prefabReference;
            Parent = new OptionT<Transform>(parent);
            OnReturnReset = new OptionT<Action<GameObject>>(onReturnPool);
        }

        public AddressCreationPoolArg(AddressPoolConfigData data) : this(data.Name, data.Amount, data.Reference, data.Parent) { }
    }

    public class AddressPoolManager : MonoBehaviour
    {
        private readonly Dictionary<string, AddressPoolData> _poolData = new();
        private static OptionT<AddressPoolManager> _instance;
        private static readonly object _lock = new();
        private OptionT<Transform> _trm;

        public static AddressPoolManager Instance
        {
            get
            {
                lock (_lock)
                    return _instance.Get(FindOrCreateDispatcher());
            }
        }

        /// <summary>
        /// Create a new pool with the given AssetReferenceGameObject.
        ///     - Check and save a reference for the transform.
        ///     - Add deactivation of the object to the OnReturnReset.
        ///     - Generate a PoolData and add it to the pools.
        ///     - Create instances.
        /// </summary>
        /// <param name="args"></param>
        public void CreatePool(AddressCreationPoolArg args)
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

            var poolData = new AddressPoolData()
            {
                Prefab = args.Reference,
                Parent = args.Parent,
                OnReturnReset = onReturn,
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
        public bool PoolExist(string poolName) => _poolData.ContainsKey(poolName);

        /// <summary>
        /// Get an instance from the pool, how the creation is async need a callback to set the object.
        ///     - Check if the pool exist.
        ///     - If the pool is empy, create a new instance async.
        ///     - Excecute the callback with the instance.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="getCallback"></param>
        public void GetInstance(string poolName, Action<GameObject> getCallback)
        {
            PoolExistChecker(poolName, "get instance");

            var data = _poolData[poolName];
            if (!data.AvalibleObjects.Any())
            {
                CreateInstance(poolName, data, true, new OptionT<Action<GameObject>>(getCallback));
                return;
            }
            var instance = data.AvalibleObjects.Pop();
            instance.SetActive(true);
            getCallback(instance);
        }

        /// <summary>
        /// Get an instance from the pool, if the pool is empty will return a null option.
        ///     - Check if the pool exist.
        ///     - Get the object from the pool, if is empty return a null option.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public OptionT<GameObject> GetInstanceUnsafe(string poolName)
        {
            PoolExistChecker(poolName, "get instance unsafe");

            var data = _poolData[poolName];
            var instance = data.AvalibleObjects.Any() ? data.AvalibleObjects.Pop() : null;
            return new OptionT<GameObject>(instance);
        }

        /// <summary>
        /// Get an instance async from the pool, returning a Task of the operation.
        ///     - Check if pool exist.
        ///     - Check if the pool is empty and return an instance or create a new one.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public async Task<GameObject> GetInstanceAsync(string poolName)
        {
            PoolExistChecker(poolName, "get instance async");

            var data = _poolData[poolName];
            return data.AvalibleObjects.Any() ? data.AvalibleObjects.Pop() : await CreateInstanceAsync(poolName, data, true);
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
        /// Release all elements of the pool from memory, can release the asset and remove the pool too.
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="removePoolAndReleaseAsset"></param>
        public void DisposePool(string poolName, bool removePoolAndReleaseAsset = true)
        {
            PoolExistChecker(poolName, "dispose pool");

            var data = _poolData[poolName];

            while (data.AvalibleObjects.Peek() != null)
                Addressables.ReleaseInstance(data.AvalibleObjects.Pop());

            if (removePoolAndReleaseAsset)
            {
                _poolData.Remove(poolName);
                data.Prefab.ReleaseAsset();
            }
        }

        /// <summary>
        /// Clean and remove all the pools from memory, can destroy the pool manager too.
        /// </summary>
        /// <param name="removePoolAndReleaseAsset"></param>
        /// <param name="destroyPoolManager"></param>
        public void DisposeAll(bool destroyPoolManager = true)
        {
            foreach (var pool in _poolData.Values)
            {
                while (pool.AvalibleObjects.Peek() != null)
                    Addressables.ReleaseInstance(pool.AvalibleObjects.Pop());
                pool.Prefab.ReleaseAsset();
            }

            _poolData.Clear();

            if (destroyPoolManager)
                Destroy(this.gameObject);
        }


        private void OnDestroy()
        {
            _instance = new OptionT<AddressPoolManager>();
            DisposeAll();
        }

        private IEnumerator DelayedReturn(AddressPoolData data, GameObject instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            data.OnReturnReset(instance);
            data.AvalibleObjects.Push(instance);
        }

        private void PoolExistChecker(string poolName, string actionMessage)
        {
            if (!PoolExist(poolName))
                PoolErrorHandler.ThrowError(EPoolExceptions.PoolDontExistException, "AddressPool", actionMessage, poolName);
        }

        private void CreateInstance(string name, AddressPoolData data, bool activate = false,  OptionT<Action<GameObject>> callback = new())
        {
            data.Prefab.InstantiateAsync(data.Parent.Get(_trm.Get()))
                .Completed += handler =>
                {
                    if (handler.Status != AsyncOperationStatus.Succeeded)
                        PoolErrorHandler.ThrowError(EPoolExceptions.InstanceCreationException, "AddressPool", $"{handler.OperationException.Message}: {handler.OperationException.StackTrace}", name);

                    var instance = handler.Result;
                    instance.SetActive(activate);
                    data.AvalibleObjects.Push(instance);

                    callback.Get(DefaultDelegates<GameObject>.EmptyCallback).Invoke(instance);
                };
        }
        private Task<GameObject> CreateInstanceAsync(string poolName, AddressPoolData data, bool activate = false)
        {
            var handler = data.Prefab.InstantiateAsync(data.Parent.Get(_trm.Get()));
            handler.Completed += h =>
            {
                if (h.Status != AsyncOperationStatus.Succeeded)
                    PoolErrorHandler.ThrowError(EPoolExceptions.InstanceCreationException, "AddressPool", $"{h.OperationException.Message}: {h.OperationException.StackTrace}", poolName);

                var instance = h.Result;
                instance.SetActive(activate);
                data.AvalibleObjects.Push(instance);
            };
            return handler.Task;
        }

        private static AddressPoolManager FindOrCreateDispatcher()
        {
            var instance = GameObject.FindObjectOfType<AddressPoolManager>();
            if (instance == null)
                instance = new GameObject("--AddressPoolDispatcher--").AddComponent<AddressPoolManager>();

            _instance = new OptionT<AddressPoolManager>(instance);
            return instance;
        }
    }
}