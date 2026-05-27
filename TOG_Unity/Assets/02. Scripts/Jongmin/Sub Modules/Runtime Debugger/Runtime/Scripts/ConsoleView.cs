using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RuntimeLogger
{
    public class ConsoleView : MonoBehaviour
    {
        [Header("Setting Options")]
        [SerializeField] private DebuggerSettings m_debug_settings;

        [Space(30f), Header("UI")]
        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup m_canvas_group;

        [Header("Log Text")]
        [SerializeField] private TMP_Text m_log_text;

        [Space(30f), Header("Hotkeys")]
        [SerializeField] private ConsoleHotkeys m_hotkeys;

        private bool m_visible = true; 

        private readonly StringBuilder m_builder = new(16 * 4096);
        private readonly ConsoleBuffer m_buffer = new();

        private void Awake()
        {
        #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (m_debug_settings && !m_debug_settings.EnableInRelease)
            {
                gameObject.SetActive(false);
                return;
            }
        #endif
        }

        private void OnEnable()
        {
            if(m_hotkeys)
            {
                m_hotkeys.ClearRequested += Clear;
                m_hotkeys.ToggleRequested += ToggleVisible;
            }

            ApplyVisible();
            RebuildText();
        }

        private void OnDisable()
        {
            if (m_hotkeys != null)
            {
                m_hotkeys.ClearRequested -= Clear;
                m_hotkeys.ToggleRequested -= ToggleVisible;
            }
        }

        private void Update()
        {
            if (m_debug_settings == null || m_log_text == null)
                return;

            int maxLines = Mathf.Max(1, m_debug_settings.MaxLines);

            while (LogCollector.TryDequeue(out var e))
            {
                if (!PassFilter(e.type))
                    continue;

                string line = ConsoleFormatter.Format(in e, m_debug_settings);
                bool overflowed = m_buffer.Add(line, e.type, maxLines);

                if (overflowed || m_debug_settings.ShowTotal)
                {
                    RebuildText();
                }
                else
                {
                    m_builder.AppendLine(line);
                    m_log_text.text = m_builder.ToString();
                }
            }
        }

        private bool PassFilter(LogType type)
        {
            return type switch
            {
                LogType.Log       => m_debug_settings.ShowLog,
                LogType.Warning   => m_debug_settings.ShowWarning,
                LogType.Error     => m_debug_settings.ShowError,
                LogType.Assert    => m_debug_settings.ShowError,
                LogType.Exception => m_debug_settings.ShowError,
                _                 => true
            };
        }

        private void RebuildText()
        {
            m_builder.Clear();

            var lines = m_buffer.Lines;
            if (lines.Count == 0)
                m_builder.AppendLine("<color=#888888><b>Cleared!</b></color>");
            else
                for (int i = 0; i < lines.Count; i++)
                    m_builder.AppendLine(lines[i]);

            if (m_debug_settings.ShowTotal)
            {
                m_builder.AppendLine();
                m_builder.AppendLine(ConsoleFormatter.CounterRichText(m_buffer.TotalLog, m_buffer.TotalWarning, m_buffer.TotalError));
            }

            string toggle_key = m_hotkeys != null ? m_hotkeys.ToggleBinding() : "Not set";
            string clear_key  = m_hotkeys != null ? m_hotkeys.ClearBinding()  : "Not set";

            m_builder.AppendLine(
                "<color=#AAAAAA><b>[" + clear_key + "]</b></color> " +
                "<color=#666666>Console Clear</color>"
            );

            m_builder.AppendLine(
                "<color=#AAAAAA><b>[" + toggle_key + "]</b></color> " +
                "<color=#666666>Console Toggle</color>"
            );

            m_log_text.text = m_builder.ToString();
        }

        public void Clear()
        {
            m_buffer.Clear(resetTotals: true);
            RebuildText();
        }

        public void ClearViewOnly()
        {
            m_buffer.Clear(resetTotals: false);
            RebuildText();
        }

        public void ToggleVisible()
        {
            m_visible = !m_visible;
            ApplyVisible();
        }

        private void ApplyVisible()
            => m_canvas_group.alpha = m_visible ? 1f : 0f;
    }
}