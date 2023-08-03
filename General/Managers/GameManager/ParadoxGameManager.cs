using UnityEngine;
using ParadoxFramework.Abstract;
using System;
using ParadoxFramework.Utilities;
using System.Collections.Generic;

namespace ParadoxFramework.General.Managers
{
    public class ParadoxGameManager : ParadoxSingleton<ParadoxGameManager>
    {
        protected override bool IsPersistant => true;
        private OptionT<ParadoxUpdateManager> _updateManager;
        private OptionT<ParadoxFixedUpdateManager> _fixedUpdateManager;
        private OptionT<ParadoxLateUpdateManager> _lateUpdateManager;
        internal readonly WaitForEndOfFrame _unsubscribeAwaiter = new();


        public void SubscribeUpdate(IUpdateManaged managedBehaviour)
        {
            if (_updateManager.IsNull())
                _updateManager = OptionT<ParadoxUpdateManager>.NewOptionWithoutCheck(gameObject.AddComponent<ParadoxUpdateManager>());

            _updateManager.Get().Subscribe(managedBehaviour);
        }

        public void UnsubscribeUpdate(IUpdateManaged managedBehaviour)
        {
            if (_updateManager.IsNull())
                throw new Exception($"Paradox GameManager: You're trying to unsubscribe an Update method, but the update queue is null.");

            _updateManager.Get().Unsubscribe(managedBehaviour);
        }


        public void SubscribeFixedUpdate(IFixedUpdateManaged managedBehaviour)
        {
            if (_fixedUpdateManager.IsNull())
                _fixedUpdateManager = OptionT<ParadoxFixedUpdateManager>.NewOptionWithoutCheck(gameObject.AddComponent<ParadoxFixedUpdateManager>());

            _fixedUpdateManager.Get().Subscribe(managedBehaviour);
        }

        public void UnsubscribeFixedUpdate(IFixedUpdateManaged managedBehaviour)
        {
            if (_fixedUpdateManager.IsNull())
                throw new Exception($"Paradox GameManager: You're trying to unsubscribe a Fixed Update method, but the update queue is null.");

            _fixedUpdateManager.Get().Unsubscribe(managedBehaviour);
        }

        public void SubscribeLateUpdate(ILateUpdateManaged managedBehaviour)
        {
            if (_lateUpdateManager.IsNull())
                _lateUpdateManager = OptionT<ParadoxLateUpdateManager>.NewOptionWithoutCheck(gameObject.AddComponent<ParadoxLateUpdateManager>());

            _lateUpdateManager.Get().Subscribe(managedBehaviour);
        }

        public void UnsubscribeLateUpdate(ILateUpdateManaged managedBehaviour)
        {
            if (_lateUpdateManager.IsNull())
                throw new Exception($"Paradox GameManager: You're trying to unsubscribe a Late Update method, but the update queue is null.");

            _lateUpdateManager.Get().Unsubscribe(managedBehaviour);
        }


        internal void DisposeUpdate()
        {
            Destroy(_updateManager.Get());
            _updateManager = new();
        }

        internal void DisposeLateUpdate()
        {
            Destroy(_lateUpdateManager.Get());
            _lateUpdateManager = new();
        }

        internal void DisposeFixedUpdate()
        {
            Destroy(_fixedUpdateManager.Get());
            _fixedUpdateManager = new();
        }
    }

    internal class SelectionEntry<T> : IDisposable where T : IManagedBehaviour
    {
        private LinkedListNode<T> _node;
        private ParadoxManagerGeneric<T> _manager;

        public SelectionEntry(LinkedListNode<T> node, ParadoxManagerGeneric<T> manager)
        {
            _node = node;
            _manager = manager;
        }

        public void Dispose()
        {
            _node.List.Remove(_node);
            _manager.CheckListAndDisposeIfEmpty();
        }
    }
}