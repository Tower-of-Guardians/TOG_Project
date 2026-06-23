using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class ForgeDataTableRow : DataTableRowBase
    {
        public int stage;
        public int growthAtk;
        public List<int> growthBoth;
        public int growthDef;
    }
}