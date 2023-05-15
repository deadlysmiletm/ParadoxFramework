///	© Copyright 2022, Lucas Leonardo Conti - DeadlySmileTM

using UnityEngine;

namespace ParadoxFramework.General.Pool
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
