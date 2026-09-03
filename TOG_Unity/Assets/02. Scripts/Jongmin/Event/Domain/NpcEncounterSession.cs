using System.Collections.Generic;

namespace Jongmin
{
    public sealed class NpcEncounterSession
    {
        private readonly HashSet<string> _activeNpcIDs = new();
        private readonly HashSet<string> _usedSpecialActionNpcIDs = new();

        public void Begin(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }

            _activeNpcIDs.Add(npcID);
            _usedSpecialActionNpcIDs.Remove(npcID);
        }

        public void End(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }

            _activeNpcIDs.Remove(npcID);
            _usedSpecialActionNpcIDs.Remove(npcID);
        }

        public bool CanUseSpecialAction(string npcID)
        {
            return !string.IsNullOrWhiteSpace(npcID) &&
                   _activeNpcIDs.Contains(npcID) &&
                   !_usedSpecialActionNpcIDs.Contains(npcID);
        }

        public void MarkSpecialActionUsed(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID) || !_activeNpcIDs.Contains(npcID))
            {
                return;
            }

            _usedSpecialActionNpcIDs.Add(npcID);
        }

        public void Clear()
        {
            _activeNpcIDs.Clear();
            _usedSpecialActionNpcIDs.Clear();
        }
    }
}
