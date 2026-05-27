using System.IO;
using UnityEditor;
using UnityEngine;

namespace JxModule
{
    public static class AssetExtension
    {
#if UNITY_EDITOR
        public static T CreateAsset<T>(string name) where T : ScriptableObject
        {
            var path = GetSelectedAssetPath();
            path = !string.IsNullOrEmpty(path) ? path : "Assets";

            var fullPath = Path.Combine(path, $"{name}.asset");
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
            
            var asset = ScriptableObject.CreateInstance<T>();
            if (asset == null)
            {
                Debug.LogError($"Fail to create{typeof(T).Name} instance.");
                return null;
            }
            
            AssetDatabase.CreateAsset(asset, uniquePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            
            DebugExtension.LogColor($"Successfully create {typeof(T).Name} instance.", Color.green);
            return asset;
        }

        public static string GetSelectedAssetPath()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                return null;
            }

            var selectedPath = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(selectedPath))
            {
                return null;
            }
            
            return AssetDatabase.IsValidFolder(selectedPath) ? selectedPath
                                                             : Path.GetDirectoryName(selectedPath);
        }
#endif
        
        public static Sprite ToSprite(this Texture2D @this)
        {
            return Sprite.Create(@this, new Rect(0, 0, @this.width, @this.height), new Vector2(0.5f, 0.5f));
        }
        
        public static Texture2D ToTexture2D(this Sprite @this)
        {
            var rect = @this.textureRect;
            var pixels = @this.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

            Texture2D result = new Texture2D((int)rect.width, (int)rect.height);
            result.SetPixels(pixels);
            result.Apply();
        
            return result;
        }
    }
}