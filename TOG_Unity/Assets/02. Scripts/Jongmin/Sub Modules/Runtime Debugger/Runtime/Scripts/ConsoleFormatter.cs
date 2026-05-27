using UnityEngine;

namespace RuntimeLogger
{
    public static class ConsoleFormatter
    {
        public static string Format(in LogCollector.Entry e, DebuggerSettings settings)
        {
            string color = e.type switch
            {
                LogType.Warning   => "#FFD700",
                LogType.Error     => "#FF4444",
                LogType.Exception => "#FF2222",
                _                 => "#00FF7F"
            };

            string prefix = e.type switch
            {
                LogType.Warning   => "[W]",
                LogType.Error     => "[E]",
                LogType.Exception => "[EX]",
                _                 => "[L]"
            };

            string time = settings != null && settings.ShowTime ? $"<color=#AAAAAA>[{e.time:HH:mm:ss}]</color> " : "";

            string message = e.m_message;

            if (settings != null && settings.ShowError &&
                (e.type == LogType.Exception || e.type == LogType.Error))
            {
                var first_line = FirstLine(e.m_stacktrace);
                if (!string.IsNullOrEmpty(first_line))
                    message += $" >>> {first_line}";
            }

            return $"{time}<color={color}><mspace=0.6em><b>{prefix,-4}</b></mspace> {message}</color>";
        }

        public static string CounterRichText(int totalLog, int totalWarn, int totalError)
        {
            string l = ConsoleBuffer.Format999Plus(totalLog);
            string w = ConsoleBuffer.Format999Plus(totalWarn);
            string e = ConsoleBuffer.Format999Plus(totalError);

            return $"<color=#00FF7F><b>L</b>: {l}</color>   " +
                   $"<color=#FFD700><b>W</b>: {w}</color>   " +
                   $"<color=#FF4444><b>E</b>: {e}</color>";
        }

        private static string FirstLine(string stacktrace)
        {
            if (string.IsNullOrEmpty(stacktrace))
                return string.Empty;

            int idx = stacktrace.IndexOf('\n');
            if (idx < 0) idx = stacktrace.IndexOf('\r');

            return idx > 0 ? stacktrace.Substring(0, idx).Trim()
                           : stacktrace.Trim();
        }
    }
}