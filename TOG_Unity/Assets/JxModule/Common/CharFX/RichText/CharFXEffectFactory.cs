using UnityEngine;

namespace JxModule.CharFX
{
    public static class CharFXEffectFactory
    {
        public static bool TryCreateEffect(in CharFXTagData tag, out ICharFXEffect effect)
        {
            effect = null;

            var name = tag.Name?.ToLowerInvariant();
            switch (name)
            {
                case "wave":
                    effect = CreateWave(tag);
                    return true;
                
                case "shake":
                    effect = CreateShake(tag);
                    return true;
                
                case "pop":
                    effect = CreatePop(tag);
                    return true;
                
                case "bounce":
                    effect = CreateBounce(tag);
                    return true;
                
                case "wiggle":
                    effect = CreateWiggle(tag);
                    return true;
                
                case "pulse":
                    effect = CreatePulse(tag);
                    return true;
                
                case "float":
                    effect = CreateFloat(tag);
                    return true;
                
                case "twist":
                    effect = CreateTwist(tag);
                    return true;
                
                case "swing":
                    effect = CreateSwing(tag);
                    return true;
                
                case "rainbow":
                    effect = CreateRainbow(tag);
                    return true;
                
                case "color":
                    effect = CreateColor(tag);
                    return true;
                
                case "fade":
                    effect = CreateFade(tag);
                    return true;
                
                default:
                    return false;
            }
        }

        private static ICharFXEffect CreateWave(CharFXTagData tag)
        {
            var amplitude = CharFXArgParser.Float(tag, 0, 5f);
            var frequency = CharFXArgParser.Float(tag, 1, 3f);
            var phaseOffset =  CharFXArgParser.Float(tag, 2, 1f);
            
            return new WaveEffect(amplitude, frequency, phaseOffset);
        }

        private static ICharFXEffect CreateShake(CharFXTagData tag)
        {
            var intensity = CharFXArgParser.Float(tag, 0, 5f);
            var frequency = CharFXArgParser.Float(tag, 1, 10f);
            var charOffset = CharFXArgParser.Float(tag, 2, 0.5f);

            return new ShakeEffect(intensity, frequency, charOffset);
        }

        private static ICharFXEffect CreatePop(CharFXTagData tag)
        {
            var duration = CharFXArgParser.Float(tag, 0, 1.5f);
            var scaleFrom = CharFXArgParser.Float(tag, 1, 0.75f);
            var stagger = CharFXArgParser.Float(tag, 2, 0.1f);
            
            return new PopEffect(duration, scaleFrom, stagger);
        }

        private static ICharFXEffect CreateBounce(CharFXTagData tag)
        {
            var height = CharFXArgParser.Float(tag, 0, 20f);
            var speed = CharFXArgParser.Float(tag, 1, 7f);
            var charOffset = CharFXArgParser.Float(tag, 2, 0f);
            
            return new BounceEffect(height, speed, charOffset);
        }

        private static ICharFXEffect CreateWiggle(CharFXTagData tag)
        {
            var angle = CharFXArgParser.Float(tag, 0, 20f);
            var speed = CharFXArgParser.Float(tag, 1, 3f);
            var charOffset = CharFXArgParser.Float(tag, 2, 0.25f);
            
            return new WiggleEffect(angle, speed, charOffset);
        }

        private static ICharFXEffect CreatePulse(CharFXTagData tag)
        {
            var scale = CharFXArgParser.Float(tag, 0, 0.15f);
            var speed = CharFXArgParser.Float(tag, 1, 4f);
            var charOffset = CharFXArgParser.Float(tag, 2, 0.25f);
            
            return new PulseEffect(scale, speed, charOffset);
        }

        private static ICharFXEffect CreateFloat(CharFXTagData tag)
        {
            var height = CharFXArgParser.Float(tag, 0, 5f);
            var speed = CharFXArgParser.Float(tag, 1, 4f);
            var xAmount = CharFXArgParser.Float(tag, 2, 5f);
            var charOffset = CharFXArgParser.Float(tag, 3, 0.35f);
            
            return new FloatEffect(height, speed, xAmount, charOffset);
        }

        private static ICharFXEffect CreateTwist(CharFXTagData tag)
        {
            var amount = CharFXArgParser.Float(tag, 0, 1f);
            var speed = CharFXArgParser.Float(tag, 1, 5f);
            var charOffset = CharFXArgParser.Float(tag, 2, 0.5f);
            
            return new TwistEffect(amount, speed, charOffset);
        }

        private static ICharFXEffect CreateSwing(CharFXTagData tag)
        {
            var angle = CharFXArgParser.Float(tag, 0, 10f);
            var speed = CharFXArgParser.Float(tag, 1, 4f);
            var charOffset = CharFXArgParser.Float(tag, 2, 0.3f);
            
            return new SwingEffect(angle, speed, charOffset);
        }

        private static ICharFXEffect CreateRainbow(CharFXTagData tag)
        {
            var speed = CharFXArgParser.Float(tag, 0, 1f);
            var saturation = CharFXArgParser.Float(tag, 1, 0.5f);
            var brightness = CharFXArgParser.Float(tag, 2, 1f);
            var charOffset = CharFXArgParser.Float(tag, 3, 0.75f);
            
            return new RainbowEffect(speed, saturation, brightness, charOffset);
        }

        private static ICharFXEffect CreateColor(CharFXTagData tag)
        {
            var color = CharFXArgParser.Color(tag, 0, Color.white);
            
            return new ColorEffect(color);
        }

        private static ICharFXEffect CreateFade(CharFXTagData tag)
        {
            var speed = CharFXArgParser.Float(tag, 0, 0f);
            var minAlpha = CharFXArgParser.Float(tag, 1, 0.5f);
            var maxAlpha = CharFXArgParser.Float(tag, 2, 1f);
            var charOffset = CharFXArgParser.Float(tag, 3, 0f);
            
            return new FadeEffect(speed, minAlpha, maxAlpha, charOffset);
        }
    }
}