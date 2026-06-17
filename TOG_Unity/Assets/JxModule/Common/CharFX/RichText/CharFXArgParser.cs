using System.Globalization;
using UnityEngine;

namespace JxModule.CharFX
{
    public static class CharFXArgParser
    {
        public static string String(in CharFXTagData tag, int index, string defaultValue)
        {
            if (tag.Arguments == null || index < 0 || index >= tag.Arguments.Count)
            {
                return defaultValue;
            }

            var value = tag.Arguments[index];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }

        public static float Float(in CharFXTagData tag, int index, float defaultValue)
        {
            var value = String(tag, index, null);

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        public static int Int(in CharFXTagData tag, int index, int defaultValue)
        {
            var value = String(tag, index, null);
            
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        public static uint UInt(in CharFXTagData tag, int index, uint defaultValue)
        {
            var value = String(tag, index, null);
            
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        public static bool Bool(in CharFXTagData tag, int index, bool defaultValue)
        {
            var value = String(tag, index, null);

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (bool.TryParse(value, out var result))
            {
                return result;
            }

            return value switch
            {
                "1" => true,
                "0" => false,
                _ => defaultValue
            };
        }

        public static Color Color(in CharFXTagData tag, int index, Color defaultValue)
        {
            var value = String(tag, index, null);

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }
            
            return ColorUtility.TryParseHtmlString(value, out var result) ? result : defaultValue;
        }

        public static Color32 Color32(in CharFXTagData tag, int index, Color32 defaultValue)
        {
            return (Color32)Color(tag, index, defaultValue);
        }
    }
}