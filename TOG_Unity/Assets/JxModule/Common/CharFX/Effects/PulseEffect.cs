using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class PulseEffect : ICharFXEffect
    {
        private readonly float _scale;
        private readonly float _speed;
        private readonly float _characterOffset;

        public PulseEffect(float scale, float speed, float characterOffset)
        {
            _scale = scale;
            _speed = speed;
            _characterOffset = characterOffset;
        }
        
        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _speed - charIndex * _characterOffset;
            var scale = 1f + Mathf.Sin(t) * _scale;
            
            quad.ScaleAround(quad.MidPoint, scale);
        }
    }
}