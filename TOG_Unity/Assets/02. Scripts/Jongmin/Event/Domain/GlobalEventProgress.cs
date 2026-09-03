using System.Collections.Generic;

namespace Jongmin
{
    public sealed class GlobalEventProgress : IMutableEventProgress
    {
        private readonly HashSet<string> _seenEventIDSet = new();

        public int MaxSingleAttackDamage { get; private set; }

        public bool HasSeen(string eventID)
        {
            return !string.IsNullOrWhiteSpace(eventID) && _seenEventIDSet.Contains(eventID);
        }

        public void MarkSeen(string eventID)
        {
            if (string.IsNullOrWhiteSpace(eventID))
            {
                return;
            }

            _seenEventIDSet.Add(eventID);
        }

        public void RecordSingleAttackDamage(int damage)
        {
            if (damage > MaxSingleAttackDamage)
            {
                MaxSingleAttackDamage = damage;
            }
        }
    }
}
