namespace JxModule.CharFX
{
    public readonly struct CharFXEffectRange
    {
        public readonly ICharFXEffect Effect;
        public readonly int StartIndex;
        public readonly int EndIndex;

        public CharFXEffectRange(ICharFXEffect effect, int startIndex, int endIndex)
        {
            Effect = effect;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public bool Contains(int charIndex)
        {
            return charIndex >= StartIndex && charIndex <= EndIndex;
        }
    }
}