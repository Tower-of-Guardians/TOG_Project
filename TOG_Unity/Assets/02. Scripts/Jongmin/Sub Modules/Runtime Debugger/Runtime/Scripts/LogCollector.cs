using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace RuntimeLogger
{
    public static class LogCollector
    {
        public struct Entry
        {
            public string m_message;
            public string m_stacktrace;
            public LogType type;
            public DateTime time;
        }

        private static readonly ConcurrentQueue<Entry> s_queue = new();
        
        public static bool Enabled { get; set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Application.logMessageReceivedThreaded -= OnLog;
            Application.logMessageReceivedThreaded += OnLog;
        } 

        private static void OnLog(string condition, string stacktrace, LogType type)
        {
            if(!Enabled)
                return;

            s_queue.Enqueue(new Entry
            {
                m_message = condition,
                m_stacktrace = stacktrace,
                type = type,
                time = DateTime.Now
            });
        }

        public static bool TryDequeue(out Entry entry)
            => s_queue.TryDequeue(out entry);
    }
}