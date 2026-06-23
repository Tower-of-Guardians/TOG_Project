using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class SpeechBubbleDataTableRow : DataTableRowBase
    {
        public BubbleTriggerType triggerType;
        public List<string> lines;
    }
}