///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;
using System.Collections.Generic;

namespace ParadoxFramework.General.Pool
{
    internal class GenericPoolData<T>
    {
        public T Prefab;
        public Func<T> OnFactory;
        public Action<T> OnPoolReturn;
        public Action<T> OnDestroy;
        public Stack<T> AvalibleObjects;
    }
}
