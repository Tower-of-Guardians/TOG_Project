using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Jongmin
{
    public class TooltipTextFormatter
    {
        private static readonly Regex TokenRegex = new(@"\{([a-zA-Z0-9_]+)\}");

        public static string Format(string template, IReadOnlyDictionary<string, object> values)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            var result = values == null
                ? template
                : TokenRegex.Replace(template, match =>
                {
                    var key = match.Groups[1].Value;

                    if (!values.TryGetValue(key, out var value) || value == null)
                    {
                        return match.Value;
                    }

                    return FormatValue(value);
                });
            
            return NormalizeNewLines(result);
        }
        
        private static string FormatValue(object value)
        {
            return value switch
            {
                float floatValue => floatValue.ToString("0.##", CultureInfo.InvariantCulture),
                double doubleValue => doubleValue.ToString("0.##", CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }
        
        private static string NormalizeNewLines(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\r\\n", "\n")
                .Replace("\\n", "\n")
                .Replace("\\r", "\n")
                .Replace("\\\\r\\\\n", "\n")
                .Replace("\\\\n", "\n")
                .Replace("\\\\r", "\n")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }
    }
}