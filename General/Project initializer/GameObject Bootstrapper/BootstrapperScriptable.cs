using UnityEngine;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ParadoxFramework.General
{
    public sealed class BootstrapperScriptable : ScriptableObject
    {
        public GameObject[] PrefabsToSpawn = new GameObject[1];
        [Tooltip("If isn't destroyed on scene load.")]
        public bool IsPersitant = false;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ExecuteOnSceneLoad()
        {
            var objs = Resources.LoadAll<BootstrapperScriptable>("");

            for (int i = 0; i < objs.Length; i++)
            {
                var bootStrap = objs[i];
                for (int j = 0; j < bootStrap.PrefabsToSpawn.Length; j++)
                {
                    var obj = GameObject.Instantiate(bootStrap.PrefabsToSpawn[j]);

                    if (bootStrap.IsPersitant)
                        DontDestroyOnLoad(obj);
                }
            }
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Paradox/Bootstrapper")]
        public static void CreateAsset()
        {
            string path = "Assets/Resources/Bootstrappers";

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder("Assets/Resources", "Bootstrappers");

            StringBuilder completePath = new StringBuilder(path);
            completePath.Append("/Bootstrapper config.asset");

            var instance = ScriptableObject.CreateInstance<BootstrapperScriptable>();
            AssetDatabase.CreateAsset(instance, completePath.ToString());
            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssetIfDirty(instance);
        }
#endif
    }
}