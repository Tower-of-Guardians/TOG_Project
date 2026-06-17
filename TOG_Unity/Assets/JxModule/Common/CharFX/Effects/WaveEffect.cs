using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class WaveEffect : ICharFXEffect
    {
        private readonly float _amplitude;
        private readonly float _frequency;
        private readonly float _phaseOffset;

        public WaveEffect(float amplitude, float frequency, float phaseOffset)
        {
            _amplitude = amplitude;
            _frequency = frequency;
            _phaseOffset = phaseOffset;
        }
        
        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var wave = Mathf.Sin(context.Time *  _frequency * charIndex * _phaseOffset) * _amplitude;
            quad.Translate(new Vector3(0f, wave, 0f));
        }
    }
}