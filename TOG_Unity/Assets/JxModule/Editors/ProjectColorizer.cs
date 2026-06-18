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
        private static readonly Color JxModuleRootColor = new Color32(245, 11, 148, 127);

        private static readonly Color[] RainbowColors =
        {
            new Color(1.00f, 0.20f, 0.20f, 0.45f),
            new Color(0.90f, 0.50f, 0.10f, 0.45f),
            new Color(1.00f, 0.85f, 0.10f, 0.45f),
            new Color(0.20f, 0.90f, 0.30f, 0.45f),
            new Color(0.20f, 0.55f, 1.00f, 0.45f),
            new Color(0.25f, 0.25f, 1.00f, 0.45f),
            new Color(0.65f, 0.25f, 1.00f, 0.45f)
        };

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

            bool isJxModuleRoot = path.Equals("Assets/JxModule", StringComparison.OrdinalIgnoreCase);
            bool isJxModuleChild = path.StartsWith("Assets/JxModule/", StringComparison.OrdinalIgnoreCase);

            if (isJxModuleRoot)
            {
                EditorGUI.DrawRect(selectionRect, JxModuleRootColor);
                return;
            }

            if (isJxModuleChild)
            {
                EditorGUI.DrawRect(selectionRect, GetJxModuleRainbowColor(path));
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

        private static Color GetJxModuleRainbowColor(string path)
        {
            var relativePath = path.Substring("Assets/JxModule/".Length);
            var split = relativePath.Split('/');

            if (split.Length == 0 || string.IsNullOrEmpty(split[0]))
            {
                return JxModuleRootColor;
            }

            var firstFolderName = split[0];

            return firstFolderName.ToLowerInvariant() switch
            {
                "common" => RainbowColors[0],
                "editor" => RainbowColors[1],
                "extensions" => RainbowColors[2],
                "prefabs" => RainbowColors[3],
                "ui" => RainbowColors[4],
                "utilities" => RainbowColors[5],
                "vfx" => RainbowColors[6],
                _ => RainbowColors[Mathf.Abs(firstFolderName.GetHashCode()) % RainbowColors.Length]
            };
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