using System;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; }
        
        public ButtonAttribute(string label)
        {
            Label = label;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawButtons();
        }

        private void DrawButtons()
        {
            var targetType = target.GetType();

            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();

                if (buttonAttribute == null)
                {
                    continue;
                }

                if (method.GetParameters().Length > 0)
                {
                    DrawInvalidButton(method, "ButtonAttribute method must have no parameters.");
                    continue;
                }

                if (method.ReturnType != typeof(void))
                {
                    DrawInvalidButton(method, "ButtonAttribute method must return void.");
                    continue;
                }

                var buttonLabel = string.IsNullOrEmpty(buttonAttribute.Label) ? ObjectNames.NicifyVariableName(method.Name)
                                                                                      : buttonAttribute.Label;

                if (GUILayout.Button(buttonLabel))
                {
                    foreach (var selectedTarget in targets)
                    {
                        method.Invoke(selectedTarget, null);
                    }
                }
            }
        }

        private void DrawInvalidButton(MethodInfo method, string message)
        {
            EditorGUILayout.HelpBox($"{method.Name}: {message}", MessageType.Warning);
        }
    }
#endif
}