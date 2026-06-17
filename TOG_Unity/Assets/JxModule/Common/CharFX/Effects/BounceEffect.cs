using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class BounceEffect : ICharFXEffect
    {
        private readonly float _height;
        private readonly float _speed;
        private readonly float _charOffset;

        public BounceEffect(float height, float speed, float charOffset)
        {
            _height = height;
            _speed = speed;
            _charOffset = charOffset;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _speed - charIndex * _charOffset;
            var y = Mathf.Abs(Mathf.Sin(t)) * _height;
            
            quad.Translate(new Vector3(0f, y, 0f));
        }
    }
}