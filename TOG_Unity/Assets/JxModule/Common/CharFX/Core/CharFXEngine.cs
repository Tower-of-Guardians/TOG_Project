using System.Collections.Generic;

namespace JxModule.CharFX
{
    public sealed class CharFXEngine
    {
        public void Apply(ICharFXTextLayout layout,
                          IReadOnlyList<CharFXEffectRange> effects,
                          in CharFXContext context)
        {
            if (layout == null || effects == null || effects.Count == 0)
            {
                return;
            }

            var charCount = layout.CharacterCount;

            for (var i = 0; i < charCount; i++)
            {
                if (!layout.IsVisible(i))
                {
                    continue;
                }

                if (!layout.TryGetQuad(i, out var quad))
                {
                    continue;
                }
                
                foreach (var range in effects)
                {
                    if (range.Effect == null)
                    {
                        continue;
                    }

                    if (!range.Contains(i))
                    {
                        continue;
                    }
                    
                    range.Effect.Apply(i, ref quad, context);
                }
            
                layout.SetQuad(i, quad);
            }
        }
    }
}