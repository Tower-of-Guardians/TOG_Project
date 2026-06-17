using UnityEngine;

namespace JxModule.CharFX
{
    public sealed class PopEffect : ICharFXEffect
    {
        private readonly float _duration;
        private readonly float _scaleFrom;
        private readonly float _stagger;

        public PopEffect(float duration, float scaleFrom, float stagger)
        {
            _duration = Mathf.Max(0.0001f, duration);
            _scaleFrom = scaleFrom;
            _stagger = Mathf.Max(0f, stagger);
        }

        public void Apply(int charIndex, ref CharQuad quad, in CharFXContext context)
        {
            var localTime = context.Time - (context.StartTime + charIndex * _stagger);

            if(localTime <= 0f)
                return;

            var t = Mathf.Repeat(localTime, _duration) / _duration;

            var eased = EaseUtility.OutBack(t);
            var scale = Mathf.Lerp(_scaleFrom, 1f, eased);

            var mid = quad.MidPoint;
            quad.ScaleAround(mid, scale);

            var up = Mathf.Lerp(12f, 0f, eased);
            quad.Translate(new Vector3(0f, up, 0f));
        }
    }
}