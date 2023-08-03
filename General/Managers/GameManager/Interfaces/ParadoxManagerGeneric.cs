using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParadoxFramework.General.Managers
{
    public abstract class ParadoxManagerGeneric<T> : MonoBehaviour, IDisposable where T : IManagedBehaviour
    {
        protected readonly LinkedList<T> _managedUpdates = new();
        protected LinkedListNode<T> _currentNode;

        internal IDisposable Subscribe(T method)
        {
            var newEntry = new SelectionEntry<T>(_managedUpdates.AddLast(method), this);
            return newEntry;
        }

        internal IDisposable Subscribe(T method, int order)
        {
            if (order > _managedUpdates.Count)
                throw new IndexOutOfRangeException($"Paradox GameManager: You're trying to add a managed update in the order {order}, but you only had {_managedUpdates.Count} subscriptions");

            if (order == 0)
                return new SelectionEntry<T>(_managedUpdates.AddFirst(method), this);

            var current = _managedUpdates.First;
            for (int i = 0; i < _managedUpdates.Count; i++)
            {
                if (i == order - 1)
                    return new SelectionEntry<T>(_managedUpdates.AddAfter(current, method), this);

                current = current.Next;
            }

            throw new Exception("Paradox GameManager: Unkown error when trying to add a new update in a specific order.");
        }

        internal void Unsubscribe(T method)
        {
            _managedUpdates.Remove(method);
            CheckListAndDisposeIfEmpty();
        }

        internal void CheckListAndDisposeIfEmpty()
        {
            if (_managedUpdates.Count > 0)
                return;

            Dispose();
        }

        public virtual void Dispose()
        {
            _currentNode = null;
            _managedUpdates.Clear();
        }
    }
}