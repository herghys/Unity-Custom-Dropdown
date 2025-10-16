using UnityEditor;
using UnityEngine;

namespace Herghys.CustomUI.CustomDropdown.Editor
{
    public class CustomDropdownMenuItem : MonoBehaviour
    {
        private const string DropdownPrefabGUID = "9f895ce19cb0d9146a7fc8a103e4bc69";
        private const string DropdownSearchablePrefabGUID = "83937f241b2ff604880f688c0aa92ab5";
        private const string DropdownSearchableMultipleItemPrefabGUID = "5fb444f19d7a8d74f9b86bdb2f3af0d9";
        private const string DropdownSearchableMultipleSubItemPrefabGUID = "6b20e19d547f40d408800450fd9c1a69";

        [MenuItem("GameObject/Herghys/Custom UI/Custom Dropdown/Dropdown", false, 10)]
        public static void CreateDropdown(MenuCommand menuCommand)
            => CreatePrefab(menuCommand, DropdownPrefabGUID);
        
        [MenuItem("GameObject/Herghys/Custom UI/Custom Dropdown/Searchable Dropdown", false, 10)]
        public static void CreateDropdownSearchable(MenuCommand menuCommand)
            => CreatePrefab(menuCommand, DropdownSearchablePrefabGUID);
        
        [MenuItem("GameObject/Herghys/Custom UI/Custom Dropdown/Searchable Dropdown Multiple Item Selection", false, 10)]
        public static void CreateDropdownSearchableMultipleItem(MenuCommand menuCommand)
            => CreatePrefab(menuCommand, DropdownSearchableMultipleItemPrefabGUID);
        
        [MenuItem("GameObject/Herghys/Custom UI/Custom Dropdown/Searchable Dropdown Multiple SubItem Selection", false, 10)]
        public static void CreateDropdownSearchableMultipleSubItem(MenuCommand menuCommand)
            => CreatePrefab(menuCommand, DropdownSearchableMultipleSubItemPrefabGUID);
        

        /// <summary>
        /// Instantiate
        /// </summary>
        private static void CreatePrefab(MenuCommand menuCommand, string prefabGuid)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError($"Prefab not found via GUID: {prefabGuid}");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab at path: {prefabPath}");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate prefab.");
                return;
            }

            instance.name = "Dropdown";

            // Unpack the prefab instance completely so it becomes independent
            PrefabUtility.UnpackPrefabInstance(
                instance,
                PrefabUnpackMode.Completely,
                InteractionMode.UserAction
            );

            GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
            Selection.activeObject = instance;

            Debug.Log($"Created and unpacked prefab instance from: {prefabPath}");
        }
    }
}
