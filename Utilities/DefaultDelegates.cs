///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;

namespace ParadoxFramework.Utilities
{
    public static class DefaultDelegates<T>
    {
        public static readonly Action<T> EmptyCallback = delegate { };
    }
}
