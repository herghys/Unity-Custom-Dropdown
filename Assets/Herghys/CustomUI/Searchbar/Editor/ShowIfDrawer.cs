using Herghys.CustomUI.Searchbar.Runtime;
using UnityEditor;
using UnityEngine;

namespace Herghys.Utility.Searchbar.Editors
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            SerializedProperty comparedProperty = property.serializedObject.FindProperty(showIf.conditionPropertyName);

            if (comparedProperty == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            bool shouldShow = false;

            switch (comparedProperty.propertyType)
            {
                case SerializedPropertyType.Enum:
                    int enumIndex = comparedProperty.enumValueIndex;
                    shouldShow = enumIndex.Equals((int)showIf.compareValue);
                    break;
                default:
                    Debug.LogWarning("ShowIfDrawer: Unsupported property type.");
                    break;
            }

            if (shouldShow)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = attribute as ShowIfAttribute;
            SerializedProperty comparedProperty = property.serializedObject.FindProperty(showIf.conditionPropertyName);

            bool shouldShow = false;

            if (comparedProperty != null && comparedProperty.propertyType == SerializedPropertyType.Enum)
            {
                int enumIndex = comparedProperty.enumValueIndex;
                shouldShow = enumIndex.Equals((int)showIf.compareValue);
            }

            return shouldShow ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
        }
    }
}