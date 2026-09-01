using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class EventRewardDataTableRow : DataTableRowBase
    {
        public EEventRewardType rewardType;
        public string targetID;
        public int amount;
        public List<string> arguments;
    }
}