using System.Collections.Generic;

namespace Jongmin
{
    public class TooltipContent
    {
        public readonly string TooltipId;
        public readonly IReadOnlyDictionary<string, object> Values;
        
        public bool IsValid => !string.IsNullOrWhiteSpace(TooltipId);

        public TooltipContent(string tooltipId, IReadOnlyDictionary<string, object> values)
        {
            TooltipId = tooltipId;
            Values = values;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (Values != null && Values.TryGetValue(key, out var rawValue) && rawValue is T typedValue)
            {
                value = typedValue;
                return true;
            }
            
            value = default;
            return false;
        }
    }
}