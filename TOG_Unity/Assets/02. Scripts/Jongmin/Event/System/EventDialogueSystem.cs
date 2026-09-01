using System;
using System.Linq;
using JxDialogueBox;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class EventDialogueSystem : MonoBehaviour
    {
        private DialogueRunner _dialogueRunner;
        private EventConditionSystem _conditionSystem;
        private EventHistory _eventHistory;
        private DataTable _eventDataTable;

        public event Action<EventDataTableRow> OnBeginEventReward;

        public void Construct(DialogueRunner dialogueRunner,
                              EventConditionSystem conditionSystem,
                              EventHistory eventHistory, 
                              DataTable eventDataTable)
        {
            _dialogueRunner = dialogueRunner;
            _conditionSystem = conditionSystem;
            _eventHistory = eventHistory;
            _eventDataTable = eventDataTable;
        }

        /// <summary>
        /// NPC와 이벤트 대화를 진행할 수 있는지 시도합니다.
        /// </summary>
        public bool TryStartEventDialogue(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID) || _dialogueRunner == null || _eventDataTable == null)
            {
                return false;
            }

            var eventDataTableRow = FindRunnableEvent(npcID);
            if (eventDataTableRow == null)
            {
                return false;
            }

            _dialogueRunner.StartDialogue(eventDataTableRow.dialogueID, () =>
            {
                OnBeginEventReward?.Invoke(eventDataTableRow);

                if (eventDataTableRow.isOnce)
                {
                    _eventHistory?.MarkSeen(eventDataTableRow.rowID);
                }
            });

            return true;
        }

        private EventDataTableRow FindRunnableEvent(string npcID)
        {
            return _eventDataTable
                .FindAll<EventDataTableRow>()
                .Where(row => row.npcID == npcID)
                .Where(row => !row.isOnce || _eventHistory == null || !_eventHistory.HasSeen(row.rowID))
                .Where(row => _conditionSystem == null || _conditionSystem.IsSatisfied(row.conditionIDs, npcID))
                .OrderBy(row => row.priority)
                .FirstOrDefault();
        }
    }
}
