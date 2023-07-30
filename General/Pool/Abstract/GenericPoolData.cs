///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;
using System.Collections.Generic;

namespace ParadoxFramework.General.Pool
{
    internal class GenericPoolData<T>
    {
        public T Prefab;
        public Stack<T> AvalibleObjects;
    }
}
