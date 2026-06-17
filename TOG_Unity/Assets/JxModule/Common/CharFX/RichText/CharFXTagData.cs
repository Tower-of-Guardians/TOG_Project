using System.Collections.Generic;

namespace JxModule.CharFX
{
    public readonly struct CharFXTagData
    {
        public readonly string Name;
        public readonly IReadOnlyList<string> Arguments;

        public CharFXTagData(string name, IReadOnlyList<string> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}