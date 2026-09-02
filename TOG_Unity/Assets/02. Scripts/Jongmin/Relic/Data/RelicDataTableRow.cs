using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class RelicDataTableRow : DataTableRowBase
    {
        public string displayName;
        public string displayImage;
        public ERelicType relicType;
        public string description;
        public List<string> effectIDs;
    }
}

