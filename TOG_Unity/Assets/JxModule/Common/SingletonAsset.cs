using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    public class SingletonAsset<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        var type = typeof(T).Name;
#if UNITY_EDITOR
                        var guid = AssetDatabase.FindAssets($"t:{type}");
                        var path = AssetDatabase.GUIDToAssetPath(guid[0]);
                        _instance = AssetDatabase.LoadAssetAtPath<T>(path);
#else
                        _instance = (T)Resources.LoadAll<T>("")[0];
#endif
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"SingletonAsset: {e.StackTrace}");
                        return null;
                    }
                }
                
                return _instance;
            }
        }
    }
}