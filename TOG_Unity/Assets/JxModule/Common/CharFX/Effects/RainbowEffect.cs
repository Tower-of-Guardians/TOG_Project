using UnityEngine;

namespace JxModule.CharFX
{
    public class RainbowEffect : ICharFXEffect
    {
        private readonly float _speed;
        private readonly float _saturation;
        private readonly float _brightness;
        private readonly float _characterOffset;


        public RainbowEffect(float speed, float saturation, float brightness, float characterOffset)
        {
            _speed = speed;
            _saturation = saturation;
            _brightness = brightness;
            _characterOffset = characterOffset;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var hue = context.ElapsedTime * _speed + charIndex * _characterOffset;
            hue = Mathf.Repeat(hue, 1f);
            
            var color = Color.HSVToRGB(hue, _saturation, _brightness);
            quad.SetColor(color);
        }
    }
}