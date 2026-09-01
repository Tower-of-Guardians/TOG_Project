using System;
using JxDialogueBox;
using UnityEngine;

namespace Jongmin
{
    public class EventDialogueSystem : MonoBehaviour
    {
        private DialogueRunner _dialogueRunner;

        public void Construct(DialogueRunner dialogueRunner)
        {
            _dialogueRunner = dialogueRunner;
        }
        
        public bool StartEventDialogue(EventDataTableRow eventDataTableRow, Action onEnded)
        {
            if (eventDataTableRow == null || string.IsNullOrWhiteSpace(eventDataTableRow.dialogueID) || _dialogueRunner == null)
            {
                return false;
            }

            _dialogueRunner.StartDialogue(eventDataTableRow.dialogueID, onEnded);
            return true;
        }
    }
}
