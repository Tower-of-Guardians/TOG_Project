using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class FloatEffect : ICharFXEffect
    {
        private readonly float _height;
        private readonly float _speed;
        private readonly float _xAmount;
        private readonly float _characterOffset;

        public FloatEffect(float height, float speed, float xAmount, float characterOffset)
        {
            _height = height;
            _speed = speed;
            _xAmount = xAmount;
            _characterOffset = characterOffset;
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var t = context.ElapsedTime * _speed + charIndex * _characterOffset;

            var x = Mathf.Sin(t * 0.7f) * _xAmount;
            var y = Mathf.Sin(t) * _height;

            quad.Translate(new Vector3(x, y, 0f));
        }
    }
}