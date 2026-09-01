using System.Linq;
using JxDialogueBox;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class RegularDialogueSystem : MonoBehaviour
    {
        private DialogueRunner _dialogueRunner;
        private IMutableDialogueProgress _dialogueProgress;
        private DataTable _dialogueDataTable;

        public void Construct(DialogueRunner dialogueRunner,
                              IMutableDialogueProgress dialogueProgress,
                              DataTable dialogueDataTable)
        {
            _dialogueRunner = dialogueRunner;
            _dialogueProgress = dialogueProgress;
            _dialogueDataTable = dialogueDataTable;
        }

        /// <summary>
        /// NPC와 정규 대화를 진행할 수 있는지 시도합니다.
        /// </summary>
        public bool TryStartRegularDialogue(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID) ||
                _dialogueRunner == null ||
                _dialogueProgress == null ||
                _dialogueDataTable == null)
            {
                return false;
            }

            var step = _dialogueProgress.GetStep(npcID);
            var dialogueDataTableRow = FindDialogue(npcID, step);
            if (dialogueDataTableRow == null)
            {
                return false;
            }

            _dialogueRunner.StartDialogue(dialogueDataTableRow.dialogueID, () => _dialogueProgress.AdvanceStep(npcID));
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
