using System.Collections.Generic;
using UnityEngine;

namespace Jongmin
{
    public class SynergyTooltipProvider : MonoBehaviour, ITooltipProvider
    {
        private SynergyTotalData _entry;

        public bool CanShowTooltip => _entry?.synergyData != null;

        public void SetEntry(SynergyTotalData entry)
        {
            _entry = entry;
        }

        public TooltipContent GetTooltipContent()
        {
            var synergyData = _entry?.synergyData;
            var tooltipDict = new Dictionary<string, object>
            {
                { "entry", _entry },
                { "synergyName", synergyData?.Name ?? string.Empty },
                { "synergyDescription", GetDescription(_entry) },
                { "currentCount", _entry?.count ?? 0 }
            };

            return new TooltipContent("UI_Synergy", tooltipDict);
        }

        private static string GetDescription(SynergyTotalData entry)
        {
            var synergyData = entry?.synergyData;
            if (synergyData == null)
            {
                return string.Empty;
            }

            var description = synergyData.Description ?? string.Empty;
            return ApplyEffectValues(description, synergyData, entry.count);
        }

        private static string ApplyEffectValues(string template, SynergyData synergyData, int currentCount)
        {
            var effect1Value = GetEffectValueAtCurrentCount(synergyData?.Effect1Synergys, currentCount);
            var effect2Value = GetEffectValueAtCurrentCount(synergyData?.Effect2Synergys, currentCount);
            var effect3Value = GetEffectValueAtCurrentCount(synergyData?.Effect3Synergys, currentCount);

            return template.Replace("{0}", effect1Value.ToString())
                           .Replace("{1}", effect2Value.ToString())
                           .Replace("{2}", effect3Value.ToString());
        }

        private static int GetEffectValueAtCurrentCount(IReadOnlyList<int> effectValues, int currentCount)
        {
            if (effectValues == null || effectValues.Count == 0 || currentCount <= 0)
            {
                return 0;
            }

            var index = Mathf.Clamp(currentCount - 1, 0, effectValues.Count - 1);
            return effectValues[index];
        }
    }
}
