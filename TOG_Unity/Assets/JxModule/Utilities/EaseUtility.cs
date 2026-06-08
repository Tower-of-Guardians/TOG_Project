using UnityEngine;

namespace JxModule
{
    public static class EaseUtility
    {
        public static float Linear(float t)
        {
            return Mathf.Clamp01(t);
        }

        public static float InQuad(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t;
        }

        public static float OutQuad(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - (1f - t) * (1f - t);
        }

        public static float InOutQuad(float t)
        {
            t = Mathf.Clamp01(t);

            if (t < 0.5f)
            {
                return 2f * t * t;
            }

            return 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        public static float InCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t;
        }

        public static float OutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public static float InOutCubic(float t)
        {
            t = Mathf.Clamp01(t);

            if (t < 0.5f)
            {
                return 4f * t * t * t;
            }

            return 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static float OutBack(float t, float overshoot = 1.70158f)
        {
            t = Mathf.Clamp01(t);
            float x = t - 1f;
            return 1f + (overshoot + 1f) * x * x * x + overshoot * x * x;
        }

        public static float InBack(float t, float overshoot = 1.70158f)
        {
            t = Mathf.Clamp01(t);
            return (overshoot + 1f) * t * t * t - overshoot * t * t;
        }
    }
}