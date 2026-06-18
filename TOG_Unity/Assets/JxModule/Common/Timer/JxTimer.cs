using System;
using UnityEngine;

namespace JxModule.Timer
{
    public class JxTimer
    {
        private float _targetTime;
        private float _elapsedTime;
        private bool _isRunning;

        public event Action OnComplete;

        public float Duration => _targetTime;
        public float Elapsed => _elapsedTime;
        public float Remaining => Mathf.Max(0f, _targetTime - _elapsedTime);
        public float Progress => _targetTime <= 0f ? 1f : Math.Min(_elapsedTime / _targetTime, 1f);
        public bool IsRunning => _isRunning;
        public bool IsComplete => _elapsedTime >= _targetTime;

        public JxTimer()
        {
            _targetTime = 0f;
        }

        public JxTimer(float duration)
        {
            _targetTime = Mathf.Max(0f, duration);
        }

        public void SetTimer(float duration)
        {
            _targetTime = duration;
            _elapsedTime = 0f;
            _isRunning = true;
        }

        public void Start()
        {
            _elapsedTime = 0f;
            _isRunning = true;
        }

        public void Stop()
        {
            _elapsedTime = 0f;
            _isRunning = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_isRunning)
            {
                return;
            }
            
            _elapsedTime += deltaTime;
            if (!(_elapsedTime >= _targetTime))
            {
                return;
            }
            
            _elapsedTime = _targetTime;
            _isRunning = false;
            OnComplete?.Invoke();
        }
    }
}