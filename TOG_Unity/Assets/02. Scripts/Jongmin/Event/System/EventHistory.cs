using System.Collections.Generic;

namespace Jongmin
{
    public sealed class EventHistory : IEventProgress
    {
        private readonly HashSet<string> _seenEventIDSet = new();

        /// <summary>
        /// 이벤트를 이미 실행한 적이 있는지 확인합니다.
        /// </summary>
        public bool HasSeen(string eventID)
        {
            return !string.IsNullOrWhiteSpace(eventID) && _seenEventIDSet.Contains(eventID);
        }

        /// <summary>
        /// 이미 실행한 이벤트를 등록합니다.
        /// </summary>
        public void MarkSeen(string eventID)
        {
            if (string.IsNullOrWhiteSpace(eventID))
            {
                return;
            }

            _seenEventIDSet.Add(eventID);
        }
    }
}