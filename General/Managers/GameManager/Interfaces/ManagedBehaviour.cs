using System;

namespace ParadoxFramework.General.Managers
{
    public interface IManagedBehaviour
    {
        bool IsObjectActive { get; }
    }

    public interface IAwakeManged
    {
        void ManagedAwake();
    }

    public interface IStartManaged : IManagedBehaviour
    {
        void ManagedStart();
    }

    public interface IUpdateManaged : IManagedBehaviour
    {
        void ManagedUpdate();
    }

    public interface IFixedUpdateManaged : IManagedBehaviour
    {
        void ManagedFixedUpdate();
    }

    public interface ILateUpdateManaged : IManagedBehaviour
    {
        void ManagedLateUpdate();
    }

    public interface IParadoxManager<T> where T : IManagedBehaviour
    {
        IDisposable SubscribeUpdate(T update);
        IDisposable SubscribeUpdate(T update, int order);
        void UnsubscribeUpdate(T update);
        void CheckListAndDisposeIfEmpty();
    }
}