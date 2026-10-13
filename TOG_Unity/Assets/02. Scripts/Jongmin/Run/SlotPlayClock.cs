using System;

namespace Jongmin
{
    public sealed class SlotPlayClock
    {
        private double _lastTime;
        private double _remainder;
        public bool IsRunning { get; private set; }

        public void Start(double now)
        {
            _lastTime = now;
            _remainder = 0;
            IsRunning = true;
        }

        public long CollectSeconds(double now)
        {
            if (!IsRunning)
            {
                return 0;
            }
            
            var elapsed = Math.Max(0, now - _lastTime) + _remainder;
            var seconds = (long)Math.Floor(elapsed);
            
            _remainder = elapsed - seconds;
            _lastTime = now;
            
            return seconds;
        }

        public void Stop() => IsRunning = false;
    }
}
