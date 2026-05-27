using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class BigHeaderAttribute : PropertyAttribute
    {
        public string Text { get; }

        public BigHeaderAttribute(string text)
        {
            Text = text;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BigHeaderAttribute))]
    public class BigHeaderAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            if (attribute is not BigHeaderAttribute attributeHandle)
            {
                return;
            }
            
            position = EditorGUI.IndentedRect(position);
            position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;

            var bigHeaderStyle = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.white
                }
            };
            
            GUI.Label(position, attributeHandle.Text, bigHeaderStyle);
            EditorGUI.DrawRect(new Rect(position.xMin, position.yMin - 5, position.width, 1), Color.white);
        }
    }
#endif
}