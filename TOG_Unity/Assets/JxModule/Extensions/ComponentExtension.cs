using UnityEngine;

namespace JxModule
{
    public static class ComponentExtension
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        public static bool HasComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.HasComponent<T>();
        }

        public static bool TryGetComponentInParent<T>(this Component component, out T result) where T : Component
        {
            result = component.GetComponentInParent<T>();
            return result != null;
        }

        public static bool TryGetComponentInChildren<T>(this Component component, out T result) where T : Component
        {
            result = component.GetComponentInChildren<T>();
            return result != null;
        }

        public static void SetActiveSafe(this Component component, bool active)
        {
            component.gameObject.SetActiveSafe(active);
        }

        public static void ToggleActive(this Component component)
        {
            component.gameObject.ToggleActive();
        }

        public static void DestroyGameObject(this Component component)
        {
            Object.Destroy(component.gameObject);
        }

        public static void DestroyGameObjectImmediate(this Component component)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }
}