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

        public void ShowLine(BubbleTriggerType triggerType, int optIndex = 0)
        {
            var line = _speechLineTable.Find<SpeechBubbleDataTableRow>(x => x.triggerType == triggerType);
            _view?.SetLineLabel(line.lines[optIndex]);
        }
    }
}