namespace JxModule.CharFX
{
    public interface ICharFXTextLayout
    {
        int CharacterCount { get; }

        bool IsVisible(int charIndex);
        bool TryGetQuad(int charIndex, out CharQuad quad);
        void SetQuad(int charIndex, in CharQuad quad);
    }
}