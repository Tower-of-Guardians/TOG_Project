using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    public class SceneOnlyAttribute : PropertyAttribute
    {
        public readonly string Message;

        public SceneOnlyAttribute(string message = "Scene object only.")
        {
            Message = message;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneOnlyAttribute))]
    public class SceneOnlyAttributeDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> InvalidStateByProperty = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attributeData = (SceneOnlyAttribute)attribute;
            var key = GetPropertyKey(property);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                InvalidStateByProperty[key] = true;

                DrawPropertyField(position, property, label);
                DrawHelpBox(position, "SceneOnly can only be used on ObjectReference fields.", MessageType.Warning);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            var currentValue = property.objectReferenceValue;

            EditorGUI.BeginChangeCheck();

            var newValue = EditorGUI.ObjectField(
                fieldRect,
                label,
                currentValue,
                fieldInfo.FieldType,
                true
            );

            var changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                var isInvalid = newValue != null && EditorUtility.IsPersistent(newValue);

                InvalidStateByProperty[key] = isInvalid;

                if (!isInvalid)
                {
                    property.objectReferenceValue = newValue;
                }
                else
                {
                    GUI.changed = true;
                }
            }

            if (IsInvalidState(key))
            {
                DrawHelpBox(position, attributeData.Message, MessageType.Error);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var key = GetPropertyKey(property);

            if (IsInvalidState(key))
            {
                return GetHeightWithHelpBox();
            }

            return EditorGUIUtility.singleLineHeight;
        }

        private static bool IsInvalidState(string key)
        {
            return InvalidStateByProperty.TryGetValue(key, out bool isInvalid) && isInvalid;
        }

        private static string GetPropertyKey(SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";
        }

        private static float GetHeightWithHelpBox()
        {
            return EditorGUIUtility.singleLineHeight
                   + EditorGUIUtility.standardVerticalSpacing
                   + EditorGUIUtility.singleLineHeight * 1.4f;
        }

        private static void DrawPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(fieldRect, property, label, true);
        }

        private static void DrawHelpBox(Rect position, string message, MessageType messageType)
        {
            var helpBoxRect = position;
            helpBoxRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            helpBoxRect.height = EditorGUIUtility.singleLineHeight * 1.4f;

            EditorGUI.HelpBox(helpBoxRect, message, messageType);
        }
    }
#endif
}