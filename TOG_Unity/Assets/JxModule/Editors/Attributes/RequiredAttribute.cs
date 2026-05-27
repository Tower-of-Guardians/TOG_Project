using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    public class RequiredAttribute : PropertyAttribute
    {
        public readonly string Message;

        public RequiredAttribute(string message = "Required reference is missing.")
        {
            Message = message;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var requiredAttribute = (RequiredAttribute)attribute;

            var fieldRect = position;
            fieldRect.height = EditorGUI.GetPropertyHeight(property, label, true);
            
            EditorGUI.PropertyField(fieldRect, property, label, true);

            if (IsMissingReference(property))
            {
                var helpBoxRect = position;
                helpBoxRect.y += fieldRect.height + EditorGUIUtility.standardVerticalSpacing;
                helpBoxRect.height = EditorGUIUtility.singleLineHeight * 1.4f;
                
                EditorGUI.HelpBox(helpBoxRect, requiredAttribute.Message, MessageType.Error);
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property, label, true);

            if (IsMissingReference(property))
            {
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight * 1.4f;
            }

            return height;
        }

        private static bool IsMissingReference(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null;
        }
    }
#endif
}