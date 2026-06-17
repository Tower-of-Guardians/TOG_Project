namespace JxModule.CharFX
{
    public interface ICharFXEffect
    {
        void Apply(int charIndex, ref CharQuad quad, in CharFXContext context);
    }
}