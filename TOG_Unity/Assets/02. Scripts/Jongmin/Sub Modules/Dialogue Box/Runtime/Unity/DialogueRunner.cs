using System;
using JxModule;
using UnityEngine;

namespace JxDialogueBox
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        [BigHeader("Dialogue View")]
        [SerializeField, Required] private DialogueView dialogueView;

        private DialogueEngine _dialogueEngine;
        private Action _onDialogueEnded;

        private void Awake()
        {
            if(dialogueView == null)
            {
                enabled = false;
                return;
            }

            var dataSource = new DialogueTableDataSource();
            _dialogueEngine = new DialogueEngine(dataSource);
            
            _dialogueEngine.OnLine += HandleLine;
            _dialogueEngine.OnChoice += HandleChoice;
            _dialogueEngine.OnEnded += HandleEnded;
        }

        private void Start()
        {
            dialogueView.Bind(onNextAction:   () => _dialogueEngine.Advance(),
                              onChooseAction: (idx) => _dialogueEngine.Choose(idx));
        }

        public void StartDialogue(string dialogueID, Action onEnded = null)
        {
            _onDialogueEnded = onEnded;
            dialogueView.OpenView();
            _dialogueEngine.Start(dialogueID);
        }

        private void HandleLine(DialogueEngine.LineEvent e)
        {
            dialogueView.ShowLine(e.Speaker, e.Text, e.PortraitKey);
        }

        private void HandleChoice(DialogueEngine.ChoiceEvent e)
        {
            dialogueView.ShowChoice(e.Prompt, e.Options);
        }

        private void HandleEnded()
        {
            var onEnded = _onDialogueEnded;

            dialogueView.CloseView();
            _onDialogueEnded = null;

            onEnded?.Invoke();
        }

        private void OnDestroy()
        {
            if (_dialogueEngine == null)
            {
                return;
            }

            _dialogueEngine.OnLine -= HandleLine;
            _dialogueEngine.OnChoice -= HandleChoice;
            _dialogueEngine.OnEnded -= HandleEnded;
        }
    }
}
