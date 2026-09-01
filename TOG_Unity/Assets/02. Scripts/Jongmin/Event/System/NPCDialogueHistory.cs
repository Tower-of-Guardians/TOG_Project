using System.Collections.Generic;

namespace Jongmin
{
    public sealed class NPCDialogueHistory : IDialogueProgress
    {
        private readonly Dictionary<string, int> _steps = new();

        /// <summary>
        /// NPC와 몇 번째 대화를 진행해야 하는지를 반환합니다.
        /// </summary>
        public int GetStep(string npcID)
        {
            return !string.IsNullOrWhiteSpace(npcID) && 
                   _steps.TryGetValue(npcID, out var step) ? step : 0;
        }

        /// <summary>
        /// NPC와의 대화를 마치고 진행도를 업데이트합니다.
        /// </summary>
        public void AdvanceStep(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }

            _steps[npcID] = GetStep(npcID) + 1;
        }
    }
}