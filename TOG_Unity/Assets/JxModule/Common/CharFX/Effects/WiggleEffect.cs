using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class WiggleEffect : ICharFXEffect
    {
        private readonly float _angle;
        private readonly float _speed;
        private readonly float _characterOffset;

        public WiggleEffect(float angle, float speed, float characterOffset)
        {
            _angle = angle;
            _speed = speed;
            _characterOffset = characterOffset;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _speed - charIndex * _characterOffset;
            var angle = Mathf.Sin(t) * _angle;
            
            quad.RotateAround(quad.MidPoint, angle);
        }
    }
}