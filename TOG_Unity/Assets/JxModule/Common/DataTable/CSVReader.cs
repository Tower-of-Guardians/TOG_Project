using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace JxModule.DataTable
{
    public class CSVReader
    {
        private static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

        public static List<Dictionary<string, string>> Read(TextAsset csv, char trim = ',')
        {
            var rowContents = new List<Dictionary<string, string>>();
            var rawContent = csv.text;
            
            var lines = Regex.Split(rawContent, LINE_SPLIT_RE);
            if (lines.Length <= 1)
            {
                return rowContents;
            }
            
            var header = Regex.Split(lines[0], SPLIT_RE);
            if (header.Length > 0)
            {
                header[0] = header[0].Trim(new char[] { '\uFEFF', '\u2008' });
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var rowValues = Regex.Split(lines[i], SPLIT_RE);
                if (rowValues.Length == 0 || string.IsNullOrEmpty(rowValues[0]))
                {
                    continue;
                }
                
                var rowContent = new Dictionary<string, string>();
                for (var j = 0; j < header.Length && j < rowValues.Length; j++)
                {
                    var rowValue = rowValues[j];
                    rowValue = rowValue.Trim('\"').Replace("\\", "");
                    rowContent.Add(header[j], rowValue);
                }
                rowContents.Add(rowContent);
            }
            
            return rowContents;
        }
    }
}