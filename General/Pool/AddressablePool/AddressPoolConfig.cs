///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace ParadoxFramework.General.Pool
{
    [Serializable]
    public struct AddressPoolConfigData
    {
        public string Name;
        public int Amount;
        public AssetReferenceGameObject Reference;
        public Transform Parent;
    }
    [CreateAssetMenu(fileName = "AddressPoolConfig", menuName = "Paradox Framework/Pool/Addressables Config")]
    public class AddressPoolConfig : ScriptableObject
    {
        public AddressPoolConfigData[] poolConfig = new AddressPoolConfigData[1];
    }
}
