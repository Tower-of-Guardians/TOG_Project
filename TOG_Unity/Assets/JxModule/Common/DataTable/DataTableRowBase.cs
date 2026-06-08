using System.Collections.Generic;
using UnityEngine;

namespace JxModule.DataTable
{
    public abstract class DataTableRowBase : ScriptableObject
    {
        private Dictionary<string, string> _rawData;
        public string rowID;
        public bool isEnable;
        
        public void SetRawData(Dictionary<string, string> row)
        {
            _rawData = row;
        }

        public Dictionary<string, string> GetRawData()
        {
            return _rawData;
        }
    }
}