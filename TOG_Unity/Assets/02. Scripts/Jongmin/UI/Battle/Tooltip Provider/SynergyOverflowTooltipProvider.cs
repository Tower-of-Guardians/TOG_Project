using System.Collections.Generic;
using UnityEngine;

namespace Jongmin
{
    public class SynergyOverflowTooltipProvider : MonoBehaviour, ITooltipProvider
    {
        private List<SynergyTotalData> _overflowEntries;

        public bool CanShowTooltip => _overflowEntries is { Count: > 0 };

        public void SetEntries(List<SynergyTotalData> overflowEntries)
        {
            _overflowEntries = overflowEntries;
        }

        public TooltipContent GetTooltipContent()
        {
            var tooltipDict = new Dictionary<string, object>
            {
                { "overflowEntries", _overflowEntries }
            };

            return new TooltipContent("UI_Synergy", tooltipDict);
        }
    }
}