namespace JxModule.CharFX
{
    public readonly struct CharFXContext
    {
        public readonly float Time;
        public readonly float DeltaTime;
        public readonly float StartTime;
        
        public float ElapsedTime => Time - StartTime;

        public CharFXContext(float time, float deltaTime, float startTime)
        {
            Time = time;
            DeltaTime = deltaTime;
            StartTime = startTime;
        }
    }
}