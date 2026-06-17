using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JxModule.DataTable
{
    public class CSVReader
    {
        public static List<Dictionary<string, string>> Read(TextAsset csv, char trim = ',')
        {
            var rowContents = new List<Dictionary<string, string>>();
            if (csv == null || string.IsNullOrEmpty(csv.text))
            {
                return rowContents;
            }

            var rows = ParseRows(csv.text, trim);
            if (rows.Count <= 1)
            {
                return rowContents;
            }

            var header = rows[0];
            if (header.Count > 0)
            {
                header[0] = header[0].Trim(new char[] { '\uFEFF', '\u2008' });
            }

            for (var i = 1; i < rows.Count; i++)
            {
                var rowValues = rows[i];
                if (rowValues.Count == 0 || string.IsNullOrEmpty(rowValues[0]))
                {
                    continue;
                }

                var rowContent = new Dictionary<string, string>();
                for (var j = 0; j < header.Count && j < rowValues.Count; j++)
                {
                    rowContent.Add(header[j], UnescapeCell(rowValues[j]));
                }

                rowContents.Add(rowContent);
            }

            return rowContents;
        }

        private static List<List<string>> ParseRows(string rawContent, char separator)
        {
            var rows = new List<List<string>>();
            var currentRow = new List<string>();
            var currentCell = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < rawContent.Length; i++)
            {
                var c = rawContent[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < rawContent.Length && rawContent[i + 1] == '"')
                        {
                            currentCell.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentCell.Append(c);
                    }

                    continue;
                }

                if (c == '"')
                {
                    inQuotes = true;
                    continue;
                }

                if (c == separator)
                {
                    currentRow.Add(currentCell.ToString());
                    currentCell.Clear();
                    continue;
                }

                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && i + 1 < rawContent.Length && rawContent[i + 1] == '\n')
                    {
                        i++;
                    }

                    currentRow.Add(currentCell.ToString());
                    currentCell.Clear();

                    if (currentRow.Count > 1 || !string.IsNullOrEmpty(currentRow[0]))
                    {
                        rows.Add(currentRow);
                    }

                    currentRow = new List<string>();
                    continue;
                }

                currentCell.Append(c);
            }

            currentRow.Add(currentCell.ToString());
            if (currentRow.Count > 1 || !string.IsNullOrEmpty(currentRow[0]))
            {
                rows.Add(currentRow);
            }

            return rows;
        }

        private static string UnescapeCell(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\\\", "\\")
                .Replace("\\r\\n", "\n")
                .Replace("\\n", "\n")
                .Replace("\\r", "\n")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"");
        }
    }
}
