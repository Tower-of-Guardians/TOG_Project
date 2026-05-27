using UnityEngine;

namespace RuntimeLogger
{
    [CreateAssetMenu(fileName = "Debugger Settings", menuName ="Runtime Debugger/Debugger Settings")]
    public class DebuggerSettings : ScriptableObject
    {
        [Header("Debugger Setting Options")]
        [Header("Max Log Lines")]
        [SerializeField] private int m_max_lines = 10;
        public int MaxLines => m_max_lines;

        [Header("Show Current Time")]
        [SerializeField] private bool m_show_time = true;
        public bool ShowTime => m_show_time;

        [Space(30f), Header("Debugger Filter Options")]
        [Header("Show Log")]
        [SerializeField] private bool m_show_log = true;
        public bool ShowLog => m_show_log;

        [Header("Show Warning")]
        [SerializeField] private bool m_show_warning = true;
        public bool ShowWarning => m_show_warning;

        [Header("Show Error")]
        [SerializeField] private bool m_show_error = true;
        public bool ShowError => m_show_error;

        [Header("Show Log's total count")]
        [SerializeField] private bool m_show_total = true;
        public bool ShowTotal => m_show_total;

        [Space(30f), Header("Release Options")]
        [Header("Enable in release")]
        [SerializeField] private bool m_enable_in_release = false;
        public bool EnableInRelease => m_enable_in_release;
    }
}