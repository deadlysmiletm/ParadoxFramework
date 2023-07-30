using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

using Settings = UnityEditor.AddressableAssets.Settings;

namespace ParadoxFramework.Utilities.Editor
{
    public static class AddressablessEditorUtilities
    {
        public static AddressableAssetGroup GetOrCreateAssetGroup(string groupName)
        {
            var groups = GetAllAddressableGroups();
            AddressableAssetGroup filteredGroup;

            if (FindGroupByName(groups, groupName, out filteredGroup))
                return filteredGroup;

            return CreateNewAssetGroup(groupName);
        }

        public static AddressableAssetGroup CreateNewAssetGroup(string groupName)
        {
            var group = AddressableAssetSettingsDefaultObject.Settings.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema>());
            group.AddSchema(typeof(Settings.GroupSchemas.BundledAssetGroupSchema));
            group.AddSchema(typeof(Settings.GroupSchemas.ContentUpdateGroupSchema));

            return group;
        }

        public static bool GetGroupByName(string groupName, out AddressableAssetGroup group)
        {
            var groups = GetAllAddressableGroups();

            if (FindGroupByName(groups, groupName, out group))
                return true;

            return false;
        }

        public static bool GroupExist(string groupName)
        {
            var groups = GetAllAddressableGroups();
            for (int i = 0; i < groups.Count; i++)
            {
                if (String.Equals(groups[i].Name, groupName))
                    return true;
            }

            return false;
        }

        public static void CreateAddressableEntry(UnityEngine.Object obj, AddressableAssetGroup group, string[] labels)
        {
            CreateAssetEntry(AssetDatabase.GetAssetPath(obj), group, labels);
        }


        private static AddressableAssetEntry CreateAssetEntry(string assetPath, AddressableAssetGroup group, string[] labels)
        {
            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
            
            if (labels.Length > 0)
            {
                for (int i = 0; i < labels.Length; i++)
                    entry.labels.Add(labels[i]);
            }

            return entry;
        }

        private static List<AddressableAssetGroup> GetAllAddressableGroups()
        {
            return AddressableAssetSettingsDefaultObject.Settings.groups;
        }

        private static bool FindGroupByName(List<AddressableAssetGroup> groups, string groupName, out AddressableAssetGroup selectedGroup)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (String.Equals(groupName, groups[i].Name))
                {
                    selectedGroup = groups[i];
                    return true;
                }
            }

            selectedGroup = null;
            return false;
        }
    }
}
