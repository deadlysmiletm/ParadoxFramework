///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;
using UnityEngine;

namespace ParadoxFramework.General.Pool
{
    [Serializable]
    public struct ParadoxPoolConfigData
    {
        public string Name;
        public int Amount;
        public GameObject Prefab;
    }
    [CreateAssetMenu(fileName = "ParadoxPoolConfig", menuName = "Paradox Framework/Pool/Paradox Config")]

    public class ParadoxPoolConfig : ScriptableObject
    {
        public ParadoxPoolConfigData[] Config = new ParadoxPoolConfigData[1];
    }
}
