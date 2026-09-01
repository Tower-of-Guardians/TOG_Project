using System.Linq;
using JxDialogueBox;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class RegularDialogueSystem : MonoBehaviour
    {
        private DialogueRunner _dialogueRunner;
        private NPCDialogueHistory _dialogueHistory;
        private DataTable _dialogueDataTable;

        public void Construct(DialogueRunner dialogueRunner,
                              NPCDialogueHistory dialogueHistory,
                              DataTable dialogueDataTable)
        {
            _dialogueRunner = dialogueRunner;
            _dialogueHistory = dialogueHistory;
            _dialogueDataTable = dialogueDataTable;
        }

        /// <summary>
        /// NPC와 정규 대화를 진행할 수 있는지 시도합니다.
        /// </summary>
        public bool TryStartRegularDialogue(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID) ||
                _dialogueRunner == null ||
                _dialogueHistory == null ||
                _dialogueDataTable == null)
            {
                return false;
            }

            var step = _dialogueHistory.GetStep(npcID);
            var dialogueDataTableRow = FindDialogue(npcID, step);
            if (dialogueDataTableRow == null)
            {
                return false;
            }

            _dialogueRunner.StartDialogue(dialogueDataTableRow.dialogueID, () => _dialogueHistory.AdvanceStep(npcID));
            return true;
        }

        private DialogueEntryDataTableRow FindDialogue(string npcID, int step)
        {
            return _dialogueDataTable
                .FindAll<DialogueEntryDataTableRow>()
                .Where(row => row.npcID == npcID)
                .FirstOrDefault(row => row.step == step);
        }
    }
}
