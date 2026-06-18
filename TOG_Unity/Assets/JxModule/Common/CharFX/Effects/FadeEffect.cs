using UnityEngine;

namespace JxModule.CharFX
{
    public class FadeEffect : ICharFXEffect
    {
        private readonly float _speed;
        private readonly float _minAlpha;
        private readonly float _maxAlpha;
        private readonly float _characterOffset;

        public FadeEffect(float speed, float minAlpha, float maxAlpha, float characterOffset)
        {
            _speed = speed;
            _minAlpha = minAlpha;
            _maxAlpha = maxAlpha;
            _characterOffset = characterOffset;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _speed + charIndex * _characterOffset;
            var alpha = Mathf.Lerp(_minAlpha, _maxAlpha, Mathf.PingPong(t, 1f));

            quad.MultiplyAlpha(alpha);
        }
    }
}