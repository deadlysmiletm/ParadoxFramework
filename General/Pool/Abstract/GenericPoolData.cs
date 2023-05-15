using System;
using System.Collections.Generic;
using UnityEngine;

using ParadoxFramework.Utilities;

namespace ParadoxFramework.General.Pool
{
    internal abstract class GenericPoolData<T>
    {
        public T Prefab;
        public OptionT<Transform> Parent;
        public Action<GameObject> OnReturnReset;
        public Stack<GameObject> AvalibleObjects;
    }
}