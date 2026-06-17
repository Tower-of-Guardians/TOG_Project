using System.Collections.Generic;

namespace JxModule.CharFX
{
    public sealed class CharFXParseResult
    {
        public string Text;
        public readonly List<CharFXTagRange> Ranges = new();
    }
}