using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class ShakeEffect : ICharFXEffect
    {
        private readonly float _intensity;
        private readonly float _frequency;
        private readonly float _characterOffset;

        public ShakeEffect(float intensity, float frequency, float characterOffset)
        {
            _intensity = Mathf.Max(0f, intensity);
            _frequency = Mathf.Max(0f, frequency);
            _characterOffset = characterOffset;
        }
        
        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _frequency + charIndex * _characterOffset;

            var x = Mathf.PerlinNoise(t, 0.13f) - 0.5f;
            var y = Mathf.PerlinNoise(0.37f, t) - 0.5f;

            var offset = new Vector3(x, y, 0f) * (_intensity * 2f);
            quad.Translate(offset);
        }
    }
}