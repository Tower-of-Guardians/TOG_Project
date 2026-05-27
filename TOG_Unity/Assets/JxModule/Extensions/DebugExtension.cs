using UnityEngine;

namespace JxModule
{
    public static class DebugExtension
    {
        public static void LogColor(string logText, Color color)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(color);
            Debug.Log($"<color=#{colorHex}>{logText}</color>");
        }
        
        public static void LogSeparator()
        {
            Debug.Log("<color=#888888>──────────────────────────────</color>");
        }

        public static void LogKeyValue(string key, object value)
        {
            Debug.Log($"<b><color=#90CAF9>{key}</color></b> : {value}");
        }
    }
}