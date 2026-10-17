using System;
using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace JxDialogueBox
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        [BigHeader("Dialogue View")]
        [SerializeField, Required] private DialogueView dialogueView;

        private DialogueEngine _dialogueEngine;
        private DialogueTableDataSource _dataSource;
        private Action _onDialogueEnded;
        private Coroutine _startCoroutine;
        private Coroutine _endCoroutine;

        public event Action<string, int> OnChoiceSelected;

        public bool IsRunning =>
            _startCoroutine != null ||
            _endCoroutine != null ||
            (_dialogueEngine != null &&
             (_dialogueEngine.State == DialogueEngine.EngineState.ShowingLine ||
              _dialogueEngine.State == DialogueEngine.EngineState.AwaitingChoice));

        private void Awake()
        {
            if(dialogueView == null)
            {
                enabled = false;
                return;
            }

            _dataSource = new DialogueTableDataSource();
            _dialogueEngine = new DialogueEngine(_dataSource);
            
            _dialogueEngine.OnLine += HandleLine;
            _dialogueEngine.OnChoice += HandleChoice;
            _dialogueEngine.OnEnded += HandleEnded;
        }

        private void Start()
        {
            dialogueView.Bind(onNextAction:   () => _dialogueEngine.Advance(),
                              onChooseAction: HandleChoose);
        }

        private void HandleChoose(int index)
        {
            if (_dialogueEngine.State == DialogueEngine.EngineState.AwaitingChoice)
            {
                OnChoiceSelected?.Invoke(_dialogueEngine.CurrentNodeID, index);
            }

            _dialogueEngine.Choose(index);
        }

        public void StartDialogue(string dialogueID, Action onEnded = null)
        {
            if (_startCoroutine != null)
            {
                StopCoroutine(_startCoroutine);
                _startCoroutine = null;
            }

            if (_endCoroutine != null)
            {
                StopCoroutine(_endCoroutine);
                _endCoroutine = null;
            }

            _onDialogueEnded = onEnded;
            _startCoroutine = StartCoroutine(CoStartDialogue(dialogueID));
        }

        private IEnumerator CoStartDialogue(string dialogueID)
        {
            dialogueView.PrepareDialogue(_dataSource.GetEntryCharacterID(dialogueID));

            if (_dialogueEngine.TryGetFirstLine(dialogueID, out var firstLine))
            {
                dialogueView.PrepareSpeaker(firstLine.Speaker, firstLine.PortraitKey);
            }

            yield return WaitForTween(dialogueView.Show());

            _startCoroutine = null;
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
            _onDialogueEnded = null;

            if (_endCoroutine != null)
            {
                StopCoroutine(_endCoroutine);
            }

            _endCoroutine = StartCoroutine(CoEndDialogue(onEnded));
        }

        private IEnumerator CoEndDialogue(Action onEnded)
        {
            dialogueView.ClearDialogueText();
            yield return WaitForTween(dialogueView.Hide());

            _endCoroutine = null;
            onEnded?.Invoke();
        }

        private static IEnumerator WaitForTween(Tween tween)
        {
            if (tween == null)
            {
                yield break;
            }

            yield return tween.WaitForCompletion();
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
