using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class TwistEffect : ICharFXEffect
    {
        private readonly float _amount;
        private readonly float _speed;
        private readonly float _characterOffset;

        public TwistEffect(float amount, float speed, float characterOffset)
        {
            _amount = Mathf.Clamp01(amount);
            _speed = speed;
            _characterOffset = characterOffset;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _speed + charIndex * _characterOffset;

            var wave = Mathf.Sin(t);
            var scaleX = Mathf.Lerp(1f - _amount, 1f, Mathf.Abs(wave));

            ScaleXAround(ref quad, quad.MidPoint, scaleX);
        }

        private static void ScaleXAround(ref CharQuad quad, Vector3 center, float scaleX)
        {
            quad.V0 = ScaleX(quad.V0, center, scaleX);
            quad.V1 = ScaleX(quad.V1, center, scaleX);
            quad.V2 = ScaleX(quad.V2, center, scaleX);
            quad.V3 = ScaleX(quad.V3, center, scaleX);
        }

        private static Vector3 ScaleX(Vector3 point, Vector3 center, float scaleX)
        {
            var offset = point - center;
            offset.x *= scaleX;
            return center + offset;
        }
    }
}