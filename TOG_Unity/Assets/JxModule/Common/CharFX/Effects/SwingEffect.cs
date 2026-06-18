using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class SwingEffect : ICharFXEffect
    {
        private readonly float _angle;
        private readonly float _speed;
        private readonly float _characterOffset;

        public SwingEffect(float angle, float speed, float charOffset)
        {
            _angle = angle;
            _speed = speed;
            _characterOffset = charOffset;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _speed + charIndex * _characterOffset;
            var angle = Mathf.Sin(t) * _angle;

            var pivot = (quad.V1 + quad.V2) * 0.5f;
            quad.RotateAround(pivot, angle);
        }
    }
}