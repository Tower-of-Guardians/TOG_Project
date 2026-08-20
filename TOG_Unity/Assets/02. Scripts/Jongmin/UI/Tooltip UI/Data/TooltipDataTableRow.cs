using System.Collections.Generic;
using JxModule.DataTable;

namespace Jongmin
{
    public class TooltipDataTableRow : DataTableRowBase
    {
        public ETooltipLayout layout;
        public List<string> headerText;
        public List<string> tagText;
        public List<string> bodyText;
    }
}