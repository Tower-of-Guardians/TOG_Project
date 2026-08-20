using System.Collections.Generic;
using UnityEngine;

namespace Jongmin
{
    public class DiscardButtonTooltipProvider : MonoBehaviour, ITooltipProvider
    {
        [SerializeField] private TurnManager turnManager;
        
        public bool CanShowTooltip => true;
        
        public TooltipContent GetTooltipContent()
        {
            var tooltipId = "UI_DiscardButton";
            var tooltipDict = new Dictionary<string, object>()
            {
                { "actionCount", turnManager.MaxActionCount },
                { "bodyIndex", turnManager.CanThrow ? 0 : 1 },
            };
            
            return new TooltipContent(tooltipId, tooltipDict);
        }
    }
}