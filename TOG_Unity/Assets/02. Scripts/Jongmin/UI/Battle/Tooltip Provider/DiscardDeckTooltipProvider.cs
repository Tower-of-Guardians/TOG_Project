using UnityEngine;

namespace Jongmin
{
    public class DiscardDeckTooltipProvider : MonoBehaviour, ITooltipProvider
    {
        [SerializeField] private TurnManager turnManager;
        
        public bool CanShowTooltip => true;
        
        public TooltipContent GetTooltipContent()
        {
            var tooltipId = "UI_DiscardDeck";
            return new TooltipContent(tooltipId, null);
        }
    }
}
