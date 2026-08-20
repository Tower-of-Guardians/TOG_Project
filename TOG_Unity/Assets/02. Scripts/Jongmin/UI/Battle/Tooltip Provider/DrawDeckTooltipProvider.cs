using System.Collections.Generic;
using UnityEngine;

namespace Jongmin
{
    public class DrawDeckTooltipProvider : MonoBehaviour, ITooltipProvider
    {
        [SerializeField] private TurnManager turnManager;
        
        public bool CanShowTooltip => true;
        
        public TooltipContent GetTooltipContent()
        {
            var tooltipId = "UI_DrawDeck";
            var tooltipDict = new Dictionary<string, object>()
            {
                { "cardCount", GameData.Instance.notuseDeck.Count }
            };
            
            return new TooltipContent(tooltipId, tooltipDict);
        }
    }
}