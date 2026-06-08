using UnityEngine;

namespace JxModule
{
    public static class TransformExtension
    {
        public static void SetPositionX(this Transform transform, float x)
        {
            var position = transform.position;
            position.x = x;
            transform.position = position;
        }

        public static void SetPositionY(this Transform transform, float y)
        {
            var position = transform.position;
            position.y = y;
            transform.position = position;
        }

        public static void SetPositionZ(this Transform transform, float z)
        {
            var position = transform.position;
            position.z = z;
            transform.position = position;
        }

        public static void AddPositionX(this Transform transform, float x)
        {
            transform.position += new Vector3(x, 0f, 0f);
        }

        public static void AddPositionY(this Transform transform, float y)
        {
            transform.position += new Vector3(0, y, 0f);
        }

        public static void AddPositionZ(this Transform transform, float z)
        {
            transform.position += new Vector3(0f, 0f, z);
        }

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            var localPosition = transform.localPosition;
            localPosition.x = x;
            transform.localPosition = localPosition;
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            var localPosition = transform.localPosition;
            localPosition.y = y;
            transform.localPosition = localPosition;
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            var localPosition = transform.localPosition;
            localPosition.z = z;
            transform.localPosition = localPosition;
        }

        public static void AddLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition += new Vector3(x, 0f, 0f);
        }

        public static void AddLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition += new Vector3(0, y, 0f);
        }

        public static void AddLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition += new Vector3(0f, 0f, z);
        }
        
        public static void SetLocalScaleX(this Transform transform, float x)
        {
            var scale = transform.localScale;
            scale.x = x;
            transform.localScale = scale;
        }

        public static void SetLocalScaleY(this Transform transform, float y)
        {
            var scale = transform.localScale;
            scale.y = y;
            transform.localScale = scale;
        }

        public static void SetLocalScaleZ(this Transform transform, float z)
        {
            var scale = transform.localScale;
            scale.z = z;
            transform.localScale = scale;
        }
        
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        public static void ResetLocalPosition(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
        }

        public static void ResetLocalRotation(this Transform transform)
        {
            transform.localRotation = Quaternion.identity;
        }

        public static void ResetLocalScale(this Transform transform)
        {
            transform.localScale = Vector3.one;
        }
        
        public static void DestroyChildren(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyChildrenImmediate(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        
        public static Vector2 Position2D(this Transform transform)
        {
            return transform.position;
        }

        public static Vector2 LocalPosition2D(this Transform transform)
        {
            return transform.localPosition;
        }
    }
}