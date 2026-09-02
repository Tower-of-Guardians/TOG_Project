using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class RelicEffectDataTableRow : DataTableRowBase
    {
        public ERelicEffectType effectType;
        public ERelicTriggerType triggerType;
        public ERelicValueType valueType;

        public List<string> targetIDs;
        public float value;
    }
}