using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class EventDataTableRow : DataTableRowBase
    {
        public string npcID;
        public string dialogueID;
        public int priority;
        public List<string> conditionIDs;
        public List<string> rewardIDs;
        public bool isOnce;
    }
}