using System;
using JxDialogueBox;
using UnityEngine;

namespace Jongmin
{
    public class EventDialogueSystem : MonoBehaviour
    {
        private DialogueRunner _dialogueRunner;
        private EventDomain _eventDomain;

        public event Action<EventDataTableRow> OnBeginEventReward;

        public void Construct(DialogueRunner dialogueRunner, EventDomain eventDomain)
        {
            _dialogueRunner = dialogueRunner;
            _eventDomain = eventDomain;
        }

        /// <summary>
        /// NPC와 이벤트 대화를 진행할 수 있는지 시도합니다.
        /// </summary>
        public bool TryStartEventDialogue(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID) || _dialogueRunner == null || _eventDomain == null)
            {
                return false;
            }

            var eventDataTableRow = _eventDomain.FindRunnableEvent(npcID);
            if (eventDataTableRow == null)
            {
                return false;
            }

            _dialogueRunner.StartDialogue(eventDataTableRow.dialogueID, () =>
            {
                OnBeginEventReward?.Invoke(eventDataTableRow);
                _eventDomain.MarkEventSeen(eventDataTableRow);
            });

            return true;
        }
    }
}
