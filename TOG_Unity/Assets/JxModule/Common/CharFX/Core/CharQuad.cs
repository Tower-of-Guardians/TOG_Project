using UnityEngine;

namespace JxModule.CharFX
{
    public struct CharQuad
    {
        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;
        public Vector3 V3;

        public Color32 C0;
        public Color32 C1;
        public Color32 C2;
        public Color32 C3;
        
        public Vector3 MidPoint => (V0 + V2) * 0.5f;
        
        public CharQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
                        Color32 c0, Color32 c1, Color32 c2, Color32 c3)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            V3 = v3;
            
            C0 = c0;
            C1 = c1;
            C2 = c2;
            C3 = c3;
        }

        public void Translate(Vector3 offset)
        {
            V0 += offset;
            V1 += offset;
            V2 += offset;
            V3 += offset;
        }

        public void ScaleAround(Vector3 center, float scale)
        {
            V0 = center + (V0 - center) * scale;
            V1 = center + (V1 - center) * scale;
            V2 = center + (V2 - center) * scale;
            V3 = center + (V3 - center) * scale;
        }

        public void RotateAround(Vector3 center, float angleDegrees)
        {
            var rotation = Quaternion.Euler(0f, 0f, angleDegrees);

            V0 = center + rotation * (V0 - center);
            V1 = center + rotation * (V1 - center);
            V2 = center + rotation * (V2 - center);
            V3 = center + rotation * (V3 - center);
        }
        
        public void SetColor(Color32 color, bool keepAlpha = true)
        {
            if (keepAlpha)
            {
                C0 = WithAlpha(color, C0.a);
                C1 = WithAlpha(color, C1.a);
                C2 = WithAlpha(color, C2.a);
                C3 = WithAlpha(color, C3.a);
                return;
            }

            C0 = color;
            C1 = color;
            C2 = color;
            C3 = color;
        }

        public void MultiplyAlpha(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            C0.a = (byte)(C0.a * alpha);
            C1.a = (byte)(C1.a * alpha);
            C2.a = (byte)(C2.a * alpha);
            C3.a = (byte)(C3.a * alpha);
        }
        
        private static Color32 WithAlpha(Color32 color, byte alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}