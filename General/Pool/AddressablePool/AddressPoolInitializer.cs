///	© Copyright 2022, Lucas Leonardo Conti - DeadlySmileTM

using UnityEngine;

using ParadoxFramework.General.Pool;

namespace ParadoxFramework.General
{
    public class AddressPoolInitializer : MonoBehaviour
    {
        public AddressPoolConfig data;

        private void Awake()
        {
            for (int i = 0; i < data.poolConfig.Length; i++)
                AddressPoolManager.Instance.CreatePool(new AddressCreationPoolArg(data.poolConfig[i]));

            Destroy(this.gameObject);
        }
    }
}
