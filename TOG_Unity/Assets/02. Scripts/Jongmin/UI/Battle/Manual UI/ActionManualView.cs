using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Jongmin
{
    public class ActionManualView : MonoBehaviour, ITooltipProvider
    {
        [SerializeField] private TMP_Text actionLabel;
        private int _maxActionCount;

        public bool CanShowTooltip => true;
        
        public void UpdateUI(ActionData actionData, bool isCanAction)
        {
            _maxActionCount = actionData.Max;
            
            var actionText = $"{actionData.Current} / {actionData.Max}";
            actionLabel.text = isCanAction ? actionText : $"<color=red>{actionText}</color>";
        }


        public TooltipContent GetTooltipContent()
        {
            var tooltipId = "UI_ActionManual";
            var tooltipDict = new Dictionary<string, object>()
            {
                { "actionCount", _maxActionCount },
            };
            
            return new TooltipContent(tooltipId, tooltipDict);
        }
    }
}

