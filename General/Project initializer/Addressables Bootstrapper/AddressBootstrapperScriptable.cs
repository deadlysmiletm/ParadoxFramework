using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using ParadoxFramework.Utilities.Editor;
#endif

namespace ParadoxFramework.General
{
    public sealed class AddressBootstrapperScriptable : ScriptableObject
    {
        public AssetReferenceGameObject[] Prefabs = new AssetReferenceGameObject[1];
        public bool IsPerstant = false;

        internal const string _assetGroupName = "Bootstrappers";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ExecuteOnSceneLoad()
        {
            Addressables.LoadResourceLocationsAsync(new List<string> { _assetGroupName }, Addressables.MergeMode.Intersection).Completed += LoadAssetGroup;
        }

        private static void LoadAssetGroup(AsyncOperationHandle<IList<IResourceLocation>> handler)
        {
            if (handler.Status != AsyncOperationStatus.Succeeded)
                throw new System.Exception($"Address Bootstrap: The resource location of the configuration scriptable objects wasn't loaded correctly. {handler.OperationException.Message}, {handler.OperationException.StackTrace}");

            for (int i = 0; i < handler.Result.Count; i++)
                Addressables.LoadAssetAsync<AddressBootstrapperScriptable>(handler.Result[i]).Completed += LoadBootstrapped;
        }

        private static void LoadBootstrapped(AsyncOperationHandle<AddressBootstrapperScriptable> handler)
        {
            if (handler.Status != AsyncOperationStatus.Succeeded)
                throw new System.Exception($"Addressable Bootstrap: The configuration scriptable objects wasn't loaded correctly. {handler.OperationException.Message}, {handler.OperationException.StackTrace}");

            var parent = new GameObject("--Managers--").transform;

            var config = handler.Result;
            for (int i = 0; i < config.Prefabs.Length; i++)
            {
                var h = config.Prefabs[i].InstantiateAsync(parent);
                if (config.IsPerstant)
                    h.Completed += SetObjectHasPersistant;
            }
        }

        private static void SetObjectHasPersistant(AsyncOperationHandle<GameObject> handler)
        {
            if (handler.Status != AsyncOperationStatus.Succeeded)
                throw new System.Exception($"Addressable Bootstrap: Setting an object persistent wasn't ended correctly. {handler.OperationException.Message}, {handler.OperationException.StackTrace}");
        }


#if UNITY_EDITOR
        [MenuItem("Assets/Create/Paradox/Address Bootstrapper")]
        private static void CreateAssetInProject()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(path))
                path = "Assets";

            StringBuilder completePath = new StringBuilder(path);
            completePath.Append("/Bootstrapper config.asset");

            var instance = ScriptableObject.CreateInstance<AddressBootstrapperScriptable>();
            AssetDatabase.CreateAsset(instance, completePath.ToString());
            EditorUtility.SetDirty(instance);
            AddressablessEditorUtilities.CreateAddressableEntry(instance, AddressablessEditorUtilities.GetOrCreateAssetGroup(_assetGroupName), System.Array.Empty<string>());
            AssetDatabase.SaveAssetIfDirty(instance);
        }
#endif
    }
}