using System;

using UnityEditor;
using UnityEditor.UI;

using UnityEngine;
using UnityEngine.VFX;

namespace Herghys.Utility.Searchbar.Editors
{
    [CustomEditor(typeof(SearchbarItem), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class SearchbarItemEditor : Editor
    {
        SerializedProperty m_showAccordionIndicator; //Accordion
        SerializedProperty m_dropdownAccordionIndicator; //Accordion
        SerializedProperty m_accordionSpriteState; //Accordion

        SerializedProperty m_changeGraphicsOnSelected; //Graphic
        SerializedProperty m_imageToChangeOnSelect; //Graphic
        SerializedProperty m_selectionGraphicBlock; //Graphic

        SerializedProperty m_toggleSelectionMode; //A reference to selection mode flag
        SerializedProperty m_toggle; //A Reference to Toggle
        SerializedProperty m_selectAllToggle; //A Reference to Toggle
        SerializedProperty m_contentText; //A Reference to TextMeshProUGUI
        SerializedProperty m_totalSubItemSelectedSuffix; //A Reference to TextMeshProUGUI

        SerializedProperty m_hasChildItem; //boolean
        SerializedProperty m_childItemTemplate; //A Script Reference, show if m_hasChildItem == true
        SerializedProperty m_SpawnedChildsContainer; //A Transform Reference, show if m_hasChildItem == true
        SerializedProperty m_hasAnotherItemAsParent; //A Transform Reference, show if m_hasChildItem == true
        SerializedProperty m_itemType; //Toggle Group
        SerializedProperty m_checkMark; //Toggle Group




        protected virtual void OnEnable()
        {
            m_showAccordionIndicator = serializedObject.FindProperty("m_showAccordionIndicator");
            m_dropdownAccordionIndicator = serializedObject.FindProperty("m_dropdownAccordionIndicator");
            m_accordionSpriteState = serializedObject.FindProperty("m_accordionSpriteState");

            m_changeGraphicsOnSelected = serializedObject.FindProperty("m_changeGraphicsOnSelected");
            m_imageToChangeOnSelect = serializedObject.FindProperty("m_imageToChangeOnSelect");
            m_selectionGraphicBlock = serializedObject.FindProperty("m_selectionGraphicBlock");


            m_toggleSelectionMode = serializedObject.FindProperty("m_toggleSelectionMode");
            m_toggle = serializedObject.FindProperty("m_toggle");
            m_selectAllToggle = serializedObject.FindProperty("m_SelectAllToggle");
            m_itemType = serializedObject.FindProperty("m_itemType");
            m_checkMark = serializedObject.FindProperty("m_checkMark");
            m_totalSubItemSelectedSuffix = serializedObject.FindProperty("m_TotalSubItemSelectedSuffix");
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

            SearchbarItemSettings();
            EditorGUILayout.Space();

            BaseProperty();
            EditorGUILayout.Space();

            CheckAsParent();
            EditorGUILayout.Space();

            CheckAsChild();

            serializedObject.ApplyModifiedProperties();
        }

        private void AccordionChecker()
        {
            EditorGUILayout.PropertyField(m_showAccordionIndicator, new GUIContent("Show Accordion Indicator"));
            if (m_showAccordionIndicator.boolValue)
            {
                EditorGUILayout.PropertyField(m_dropdownAccordionIndicator, new GUIContent("Dropdown indicator image"));
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_accordionSpriteState);
                --EditorGUI.indentLevel;
            }
        }

        private void CheckGraphics()
        {
            EditorGUILayout.PropertyField(m_changeGraphicsOnSelected, new GUIContent("Change Graphic On Selected"));
            if (m_changeGraphicsOnSelected.boolValue)
            {
                EditorGUILayout.PropertyField(m_imageToChangeOnSelect);
                ++EditorGUI.indentLevel;

                try
                {

                    EditorGUILayout.PropertyField(m_selectionGraphicBlock);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                //EditorGUILayout.PropertyField(m_selectionGraphicBlock);
                --EditorGUI.indentLevel;
            }
        }

        private void SearchbarItemSettings()
        {
            EditorGUILayout.LabelField("Searchbar Item Settings", EditorStyles.boldLabel);
            var itemTypeValue = (ItemType)EditorGUILayout.EnumFlagsField(
                new GUIContent("Item Type"),
                (ItemType)m_itemType.intValue
            );
            
            if ((int)itemTypeValue == 0)
                itemTypeValue = ItemType.Parent; // <- Change 'Default' to your intended fallback flag

            m_itemType.intValue = (int)itemTypeValue;
            
            var toggleSelectionValue = (ToggleSelectionMode)EditorGUILayout.EnumFlagsField(
                new GUIContent("Selection Mode"),
                (ToggleSelectionMode)m_toggleSelectionMode.intValue
            );
            
            if ((int)toggleSelectionValue == 0)
                toggleSelectionValue = ToggleSelectionMode.ShowChild;

            m_toggleSelectionMode.intValue = (int)toggleSelectionValue;
            
            if (toggleSelectionValue.HasFlag(ToggleSelectionMode.ShowChild))
            {
                AccordionChecker();
            }

            if (toggleSelectionValue.HasFlag(ToggleSelectionMode.SelectItem))
            {
                CheckGraphics();
            }
        }

        private void BaseProperty()
        {
            EditorGUILayout.PropertyField(m_toggle, new GUIContent("Item Selection Toggle"));
            EditorGUILayout.PropertyField(m_contentText, new GUIContent("Content Caption"));
            EditorGUILayout.PropertyField(m_checkMark, new GUIContent("Checkmark Reference"));
        }

        private void CheckAsParent()
        {
            EditorGUILayout.LabelField("Parent Check (Is this item a child of another item?)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_hasAnotherItemAsParent, new GUIContent("Has Another Item as parent?"));
        }

        private void CheckAsChild()
        {
            EditorGUILayout.LabelField("Child Check (Is this item a parent of another item?)", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_hasChildItem, new GUIContent("Has Child?"));
            if (m_hasChildItem.boolValue)
            {
                this.ShowChildsProperties();
            }
        }

        private void ShowChildsProperties()
        {
            EditorGUILayout.PropertyField(m_selectAllToggle, new GUIContent("Select All Sub Item Toggle"));
            EditorGUILayout.PropertyField(m_totalSubItemSelectedSuffix, new GUIContent("Selected Sub Items Suffix"));
            EditorGUILayout.PropertyField(m_childItemTemplate, new GUIContent("Child Item Template"));
            EditorGUILayout.PropertyField(m_SpawnedChildsContainer, new GUIContent("Spawned Children Container"));
        }
    }
}