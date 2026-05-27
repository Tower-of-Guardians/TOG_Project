using UnityEngine;

namespace JxModule
{
    public static class GameObjectExtension
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var component))
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }

        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out _);
        }

        public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component) where T : Component
        {
            component = gameObject.GetComponentInParent<T>();
            return component != null;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component) where T : Component
        {
            component = gameObject.GetComponentInChildren<T>();
            return component != null;
        }

        public static void SetActiveSafe(this GameObject gameObject, bool active)
        {
            if (gameObject.activeSelf == active)
            {
                return;
            }

            gameObject.SetActive(active);
        }

        public static void ToggleActive(this GameObject gameObject)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public static void DestroySelf(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }

        public static void DestroySelfImmediate(this GameObject gameObject)
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}