using JxModule.DataTable;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class SpeechBubbleDomain : MonoBehaviour
    {
        [SerializeField] private SpeechBubbleView craftmanBubbleView;
        [SerializeField] private SpeechBubbleView merchantBubbleView;
        [SerializeField] private SpeechBubbleSystem speechBubbleSystem;

        private DataTable _speechBubbleTable;
        
        private SpeechBubbleView View { get; set; }
        
        public void Construct()
        {
            _speechBubbleTable = DataTableManager.FindTable<SpeechBubbleDataTableRow>("DT_SpeechBubble");
            
            speechBubbleSystem.Construct(_speechBubbleTable);
        }

        public void OpenView(SpeechBubbleType bubbleType)
        {
            View = GetTargetView(bubbleType);
            craftmanBubbleView?.CanvasGroup.SetVisible(View == craftmanBubbleView);
            merchantBubbleView?.CanvasGroup.SetVisible(View == merchantBubbleView);
            speechBubbleSystem.SetView(View);
        }

        public void SetBubbleText(BubbleTriggerType triggerType, bool isRandom = true, int optIndex = 0, params object[] formatArgs)
        {
            speechBubbleSystem.ShowLine(triggerType, isRandom, optIndex, formatArgs);
        }

        private SpeechBubbleView GetTargetView(SpeechBubbleType bubbleType)
        {
            return bubbleType switch
            {
                SpeechBubbleType.Craftman => craftmanBubbleView,
                SpeechBubbleType.Merchant => merchantBubbleView,
                _ => null
            };
        }
    }
}
