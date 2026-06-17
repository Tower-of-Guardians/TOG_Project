namespace JxModule.CharFX
{
    public readonly struct CharFXTagRange
    {
        public readonly CharFXTagData Tag;
        public readonly int StartIndex;
        public readonly int EndIndex;

        public CharFXTagRange(CharFXTagData tag, int startIndex, int endIndex)
        {
            Tag = tag;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}