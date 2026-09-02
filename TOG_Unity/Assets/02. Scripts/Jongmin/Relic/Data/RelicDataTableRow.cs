using System.Collections.Generic;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class RelicDataTableRow : DataTableRowBase
    {
        public string displayName;
        public Texture2D displayImage;
        public ERelicType relicType;
        public string description;
        public string condition;
        public List<string> effectIDs;
    }
}

