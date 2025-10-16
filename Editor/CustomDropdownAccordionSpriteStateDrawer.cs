using System.Collections;
using System.Collections.Generic;
using Herghys.CustomUI.CustomDropdown.Core;
using UnityEditor;
using UnityEngine;

namespace Herghys.CustomUI.CustomDropdown.Editors
{
    [CustomPropertyDrawer(typeof(OpenCloseSpriteState), true)]
    public class CustomDropdownAccordionSpriteStateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty openedSprite = prop.FindPropertyRelative("m_openedSprite");
            SerializedProperty closedSprite = prop.FindPropertyRelative("m_closedSprite");

            EditorGUI.PropertyField(drawRect, openedSprite);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, closedSprite);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return 3 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
        }
    }

    [CustomPropertyDrawer(typeof(SearchbarSelectedBackground), true)]
    public class SearchbarSelectedColorBlockDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty m_normalColor = prop.FindPropertyRelative("m_normalColor");
            SerializedProperty m_selectedColor = prop.FindPropertyRelative("m_selectedColor");

            EditorGUI.PropertyField(drawRect, m_normalColor);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, m_selectedColor);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return 3 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
