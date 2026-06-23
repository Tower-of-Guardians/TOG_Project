using JxModule;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class SpeechBubbleSystem : MonoBehaviour
    {
        private SpeechBubbleView _view;
        private DataTable _speechLineTable;

        public void Construct(DataTable speechLineTable)
        {
            _speechLineTable = speechLineTable;
        }
        
        public void SetView(SpeechBubbleView view)
        {
            _view = view;
        }

        public void ShowLine(BubbleTriggerType triggerType, bool isRandom = true, int optIndex = 0)
        {
            var lineRow = _speechLineTable.Find<SpeechBubbleDataTableRow>(x => x.triggerType == triggerType);
            
            var targetDialogueLine = isRandom ? RandomUtility.GetRandom(lineRow.lines) : lineRow.lines[optIndex]; 
            _view?.SetLineLabel(targetDialogueLine);
        }
    }
}