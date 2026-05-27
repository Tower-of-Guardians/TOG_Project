using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JxModule
{
    public class PrefabManager : SingletonAsset<PrefabManager>
    {
        public List<GameObject> prefabs;

        public static T Create<T>(string name = "")
        {
            if (name.Length > 0)
            {
                var data = Instance.prefabs.Find(x => x.name == name && x.GetComponent<T>() != null);
                return Instantiate(data).GetComponent<T>();
            }
            else
            {
                var data = Instance.prefabs.Find(x => x.GetComponent<T>() != null);
                return Instantiate(data).GetComponent<T>();
            }
        }
        
        public static T CachePrefab<T>(string name = "")
        {
            if (name.Length > 0)
            {
                var data = Instance.prefabs.Find(x => x.name == name && x.GetComponent<T>() != null);
                return data.GetComponent<T>();
            }
            else
            {
                var data = Instance.prefabs.Find(x => x.GetComponent<T>() != null);
                return data.GetComponent<T>();
            }
        }
        
#if UNITY_EDITOR
        [MenuItem("Assets/JxModule/Create Prefab Manager")]
        public static void Create()
        {
            AssetExtension.CreateAsset<PrefabManager>("PrefabManager");
        }
#endif
    }
}