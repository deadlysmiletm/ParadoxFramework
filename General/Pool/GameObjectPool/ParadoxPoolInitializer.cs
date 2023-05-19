///	© Copyright 2023, Lucas Leonardo Conti - DeadlySmileTM

using UnityEngine;

using ParadoxFramework.General.Pool;

namespace ParadoxFramework.General
{
    public class ParadoxPoolInitializer : MonoBehaviour
    {
        public ParadoxPoolConfig data;

        private void Awake()
        {
            for (int i = 0; i < data.Config.Length; i++)
                ParadoxPoolManager.Instance.CreatePool(new CreationPoolArg(data.Config[i]));

            Destroy(this.gameObject);
        }
    }
}
