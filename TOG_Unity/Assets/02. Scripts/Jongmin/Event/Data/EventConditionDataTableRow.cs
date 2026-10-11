using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class EventConditionDataTableRow : DataTableRowBase
    {
        public EEventConditionType conditionType;
        public EExperienceRecordScope recordScope;
        public EExperienceRecordType recordType;
        public string targetID;
        public int value;
        public List<string> arguments;
    }
}
