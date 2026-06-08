using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    public abstract class ConditionalVisibilityAttribute : PropertyAttribute
    {
        public readonly string ConditionName;
        public readonly bool Inverted;

        protected ConditionalVisibilityAttribute(string conditionName, bool inverted)
        {
            ConditionName = conditionName;
            Inverted = inverted;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionalVisibilityAttribute), true)]
    public class ConditionalVisibilityAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property))
            {
                return;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property))
            {
                return 0f;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var visibilityAttribute = (ConditionalVisibilityAttribute)attribute;

            var conditionValue = GetConditionValue(property, visibilityAttribute.ConditionName);

            if (visibilityAttribute.Inverted)
            {
                conditionValue = !conditionValue;
            }

            return conditionValue;
        }

        private static bool GetConditionValue(SerializedProperty property, string conditionName)
        {
            var targetObject = property.serializedObject.targetObject;

            if (targetObject == null)
            {
                return true;
            }

            var targetType = targetObject.GetType();

            var fieldInfo = targetType.GetField(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
            {
                return (bool)fieldInfo.GetValue(targetObject);
            }

            var propertyInfo = targetType.GetProperty(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
            {
                return (bool)propertyInfo.GetValue(targetObject);
            }

            var methodInfo = targetType.GetMethod(conditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (methodInfo != null &&
                methodInfo.ReturnType == typeof(bool) &&
                methodInfo.GetParameters().Length == 0)
            {
                return (bool)methodInfo.Invoke(targetObject, null);
            }

            Debug.LogWarning($"ConditionalVisibility: bool field/property/method '{conditionName}' was not found on {targetType.Name}.");

            return true;
        }
    }
#endif
}