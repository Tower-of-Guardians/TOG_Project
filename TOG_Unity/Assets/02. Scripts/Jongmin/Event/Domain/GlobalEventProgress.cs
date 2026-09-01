using System.Collections.Generic;

namespace Jongmin
{
    public sealed class GlobalEventProgress : IMutableEventProgress, IMutableDialogueProgress
    {
        private readonly HashSet<string> _seenEventIDSet = new();
        private readonly Dictionary<string, int> _dialogueSteps = new();

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

        public int GetStep(string npcID)
        {
            return !string.IsNullOrWhiteSpace(npcID) &&
                   _dialogueSteps.TryGetValue(npcID, out var step) ? step : 0;
        }

        public void AdvanceStep(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }

            _dialogueSteps[npcID] = GetStep(npcID) + 1;
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
