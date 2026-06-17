using UnityEngine;

namespace JxModule.CharFX
{
    public class ColorEffect : ICharFXEffect
    {
        private readonly Color _color;
        
        public  ColorEffect(Color color)
        {
            _color = color;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            quad.SetColor(_color);
        }
    }
}