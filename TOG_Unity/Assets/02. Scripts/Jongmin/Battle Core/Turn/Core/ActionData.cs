using System;

namespace Jongmin
{
    [Serializable]
    public readonly struct ActionData
    {
        public int Current { get; }
        
        public int Max { get; }

        public ActionData(int currentActionCount, int maxActionCount)
        {
            Current = currentActionCount;
            Max = maxActionCount;
        }
    }
}