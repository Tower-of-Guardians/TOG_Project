using System.Collections.Generic;
using Jongmin;
using UnityEngine;
using UnityEngine.UI;

namespace Kwangmin
{
    public sealed class StageProgressNodeView : MonoBehaviour, ITooltipProvider
    {
        private const string VisitedTooltipId = "UI_StageAreaVisited";
        private const string LockedTooltipId = "UI_StageAreaLocked";

        [SerializeField] private Image icon;
        [SerializeField] private Image path;
        [SerializeField] private Image currentArrow;

        private StageProgressNode node;

        public bool CanShowTooltip => true;

        public void Apply(StageProgressNode state, Sprite iconSprite, Sprite pathSprite)
        {
            node = state;
            icon.sprite = iconSprite;
            path.sprite = pathSprite;
            path.gameObject.SetActive(pathSprite != null);
            currentArrow.gameObject.SetActive(state.State == EStageNodeState.Current);
        }

        public TooltipContent GetTooltipContent()
        {
            var values = new Dictionary<string, object> { { "area", node.AreaNumber } };
            if (node.State == EStageNodeState.Locked || !node.EventType.HasValue)
            {
                return new TooltipContent(LockedTooltipId, values);
            }

            values["eventName"] = GetEventName(node.EventType.Value);
            return new TooltipContent(VisitedTooltipId, values);
        }

        private static string GetEventName(AreaEventType type)
        {
            return type switch
            {
                AreaEventType.Battle => "전투",
                AreaEventType.Blacksmith => "대장간",
                AreaEventType.Shop => "상인",
                AreaEventType.Random => "랜덤 이벤트",
                AreaEventType.Blessing => "축복",
                AreaEventType.Boss => "보스",
                _ => string.Empty
            };
        }
    }
}
