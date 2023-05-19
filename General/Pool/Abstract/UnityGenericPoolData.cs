///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;
using System.Collections.Generic;
using UnityEngine;

using ParadoxFramework.Utilities;

namespace ParadoxFramework.General.Pool
{
    internal abstract class UnityGenericPoolData<T>
    {
        public T Prefab;
        public OptionT<Transform> Parent;
        public Action<GameObject> OnReturnReset;
        public Action<GameObject> OnFactoryCreation;
        public Stack<GameObject> AvalibleObjects;
    }
}