///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;

namespace ParadoxFramework.General.Pool
{
    public interface IPoolObject : IDisposable
    {
        public IPoolObject FactoryMethod();
        public void OnPoolReturn(IPoolObject obj);
    }
}
