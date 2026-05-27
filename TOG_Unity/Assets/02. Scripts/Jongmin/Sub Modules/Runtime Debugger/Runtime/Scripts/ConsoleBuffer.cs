using System.Collections.Generic;
using UnityEngine;

namespace RuntimeLogger
{
    public class ConsoleBuffer
    {
        private readonly List<string> m_lines = new();

        private int m_total_log;
        private int m_total_warning;
        private int m_total_error;

        public IReadOnlyList<string> Lines => m_lines;

        public int TotalLog => m_total_log;
        public int TotalWarning => m_total_warning;
        public int TotalError => m_total_error;

        public bool Add(string formattedLine, LogType type, int maxLines)
        {
            if (maxLines < 1) maxLines = 1;

            m_lines.Add(formattedLine);
            AddTotalCount(type);

            if (m_lines.Count > maxLines)
            {
                int removeCount = m_lines.Count - maxLines;
                m_lines.RemoveRange(0, removeCount);
                return true;
            }

            return false;
        }

        public void Clear(bool resetTotals)
        {
            m_lines.Clear();

            if (resetTotals)
            {
                m_total_log = 0;
                m_total_warning = 0;
                m_total_error = 0;
            }
        }

        private void AddTotalCount(LogType t)
        {
            switch (t)
            {
                case LogType.Log:
                    m_total_log++;
                    break;

                case LogType.Warning:
                    m_total_warning++;
                    break;

                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    m_total_error++;
                    break;
            }
        }

        public static string Format999Plus(int v) => v >= 999 ? "999+" : v.ToString();
    }
}