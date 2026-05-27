using JxModule;
using UnityEngine;

namespace JxDialogueBox
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        [BigHeader("Dialogue View")]
        [SerializeField, Required] private DialogueView dialogueView;

        private DialogueEngine _dialogueEngine;

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

        public void StartDialogue(string dialogueID)
        {
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
            dialogueView.CloseView();
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