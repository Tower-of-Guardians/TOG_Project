
using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
#endif

namespace JxModule
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class ProjectColorizer
    {
        static ProjectColorizer()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var directoryName = Path.GetFileNameWithoutExtension(path);

            if (!directoryName.Contains("scenes", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("scripts", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("prefabs", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("contents", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("images", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("fonts", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("animations", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("materials", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("shaders", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("shader graphs", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("sounds", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("audios", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("resources", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("streamingassets", StringComparison.OrdinalIgnoreCase) &&
                !directoryName.Contains("settings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            
            Color color = GetColorFromText(directoryName);
            color.a = 0.25f;

            EditorGUI.DrawRect(selectionRect, color);    
        }

        private static Color32 GetColorFromText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new Color32(125, 100, 250, 255);
            }

            var hash = text.GetHashCode();

            var r = (byte)((hash & 0xFF0000) >> 16);
            var g = (byte)((hash & 0x00FF00) >> 8);
            var b = (byte)(hash & 0x0000FF);

            return new Color32(r, g, b, 255);
        }
    }
#endif
}
