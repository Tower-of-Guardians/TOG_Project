using UnityEngine;

namespace JxModule
{
    public static class MathUtility
    {
        public static Vector2 Direction(Vector2 from, Vector2 to)
        {
            return (to - from).normalized;
        }

        public static float Angle(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        public static Vector2 DirectionFromAngle(float angle)
        {
            float radian = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public static float DistanceSqr(Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude;
        }
    }
}