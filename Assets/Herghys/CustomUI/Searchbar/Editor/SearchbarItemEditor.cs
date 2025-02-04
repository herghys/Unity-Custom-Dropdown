using UnityEditor;
using UnityEditor.UI;

using UnityEngine;

namespace Herghys.Utility.Searchbar.Editors
{
    [CustomEditor(typeof(SearchbarItem), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class SearchbarItemEditor : SelectableEditor
    {
        SerializedProperty m_toggleSelectionMode; //A reference to selection mode flag
        SerializedProperty m_toggle; //A Reference to Toggle
        SerializedProperty m_contentText; //A Reference to TextMeshProUGUI

        SerializedProperty m_hasChildItem; //boolean
        SerializedProperty m_childItemTemplate; //A Script Reference, show if m_hasChildItem == true
        SerializedProperty m_SpawnedChildsContainer; //A Transform Reference, show if m_hasChildItem == true
        SerializedProperty m_hasAnotherItemAsParent; //A Transform Reference, show if m_hasChildItem == true
        SerializedProperty m_itemType; //Toggle Group
        SerializedProperty m_checkMark; //Toggle Group

        protected override void OnEnable()
        {
            base.OnEnable();
            m_toggleSelectionMode = serializedObject.FindProperty("m_toggleSelectionMode");
            m_toggle = serializedObject.FindProperty("m_toggle");
            m_itemType = serializedObject.FindProperty("m_itemType");
            m_checkMark = serializedObject.FindProperty("m_checkMark");
            m_contentText = serializedObject.FindProperty("m_contentText");

            m_hasChildItem = serializedObject.FindProperty("m_hasChildItem");
            m_childItemTemplate = serializedObject.FindProperty("m_childItemTemplate");
            m_SpawnedChildsContainer = serializedObject.FindProperty("m_SpawnedChildsContainer");
            m_hasAnotherItemAsParent = serializedObject.FindProperty("m_hasAnotherItemAsParent");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            serializedObject.Update();

            EditorGUILayout.LabelField("Searchbar Item Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var itemTypeValue = (int)(ItemType)EditorGUILayout.EnumFlagsField(
                new GUIContent("Item Type"), (ItemType)m_itemType.intValue & ~ItemType.None);

            if (itemTypeValue == 0)
                itemTypeValue = 1;

            m_itemType.intValue = itemTypeValue;

            var toggleSelectionValue = (int)(ToggleSelectionMode)EditorGUILayout.EnumFlagsField(
                new GUIContent("Selection Mode"), (ToggleSelectionMode)m_toggleSelectionMode.intValue & ~ToggleSelectionMode.None);

            if (toggleSelectionValue == 0)
                toggleSelectionValue = 1;

            m_toggleSelectionMode.intValue = toggleSelectionValue;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_toggle, new GUIContent("Toggle"));
            EditorGUILayout.PropertyField(m_contentText, new GUIContent("Content Caption"));
            EditorGUILayout.PropertyField(m_checkMark, new GUIContent("Checkmark Reference"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Parent Check", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_hasAnotherItemAsParent, new GUIContent("Has Another Item as parent?"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Child Check", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_hasChildItem, new GUIContent("Has Child?"));
            if (m_hasChildItem.boolValue)
            {
                EditorGUILayout.PropertyField(m_childItemTemplate, new GUIContent("Child Item Template"));
                EditorGUILayout.PropertyField(m_SpawnedChildsContainer, new GUIContent("Spawned Children Container"));
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}