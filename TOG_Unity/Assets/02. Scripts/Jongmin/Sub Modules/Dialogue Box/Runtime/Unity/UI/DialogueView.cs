using System;
using JxModule;
using JxModule.DataTable;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace JxDialogueBox
{
    public class DialogueView : MonoBehaviour
    {
        [BigHeader("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private LabelBoxView nameBoxView;
        [SerializeField] private TypeWriter typeWriter;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private AdvanceButton advanceButton;

        [FormerlySerializedAs("m_settings")]
        [Space(30f), BigHeader("References")]
        [SerializeField] private DialogueSettings dialogueSettings;
        [SerializeField] private ChoiceListView choiceView;
        [SerializeField] private PortraitPanelView portraits;

        public event Action OnAdvanceRequested;

        private DataTable _characterTable;
        private Action _onNext;
        private Action<int> _onChoose;

        public bool ChoiceMode { get; private set; }

        private void Awake()
        {
            _characterTable = DataTableManager.FindTable<CharacterDataTableRow>("DT_Character");

            if (typeWriter != null && dialogueSettings != null)
            {
                typeWriter.SetInterval(dialogueSettings.typingEnabled ? dialogueSettings.typingSecondsPerCharacter : 0f);
            }

            if (typeWriter != null && advanceButton != null)
            {
                typeWriter.OnCompleted += advanceButton.SetHighlight;
            }
        }

        private void OnDestroy()
        {
            if (typeWriter != null && advanceButton != null)
            {
                typeWriter.OnCompleted -= advanceButton.SetHighlight;
            }
        }

        public void Bind(Action onNextAction, Action<int> onChooseAction)
        {
            _onNext = onNextAction;
            _onChoose = onChooseAction;

            if (advanceButton)
            {
                advanceButton.AddListener(RequestAdvance);
            }

            if (choiceView)
            {
                choiceView.Bind(index => _onChoose?.Invoke(index));
            }
        }

        public void OpenView()
            => canvasGroup.Show();

        public void CloseView()
            => canvasGroup.Hide();

        public void ShowLine(SpeakerRef speaker, string text, string portraitKey)
        {
            ClearChoice();

            if (nameBoxView)
            {
                nameBoxView.Label.text = ResolveName(speaker.CharacterID);
                nameBoxView.CanvasGroup.alpha = 1f;
            }

            if (promptLabel)
            {
                promptLabel.text = string.Empty;
            }

            if (typeWriter)
            {
                typeWriter.Play(text ?? string.Empty);
            }

            if (portraits)
            {
                portraits.ApplySpeaker(speaker, portraitKey);
            }

            if (advanceButton)
            {
                advanceButton.Show();
            }
        }

        public void ShowChoice(string prompt, ChoiceOption[] options)
        {
            ChoiceMode = true;

            
            if (nameBoxView)
            {
                nameBoxView.CanvasGroup.alpha = 0f;
                nameBoxView.Label.text = string.Empty;
            }

            if (promptLabel)
            {
                promptLabel.text = prompt ?? string.Empty;
            }

            if (typeWriter)
            {
                typeWriter.Play(string.Empty);
            }

            if (advanceButton)
            {
                advanceButton.Hide();
            }

            if (choiceView)
            {
                choiceView.Show(options);
            }
        }

        private void ClearChoice()
        {
            if (choiceView)
            {
                choiceView.Hide();
            }

            ChoiceMode = false;
        }

        public void MoveChoice(int delta)
        {
            if (!ChoiceMode)
            {
                return;
            }

            if (choiceView == null || choiceView.Count == 0)
            {
                return;
            }

            choiceView.MoveSelection(delta, true);
        }

        public void ConfirmChoice()
        {
            if (!ChoiceMode)
            {
                return;
            }

            if (choiceView == null || choiceView.Count == 0)
            {
                return;
            }

            choiceView.ConfirmSelection();
        }

        public void RequestAdvance()
        {
            if (ChoiceMode)
            {
                ConfirmChoice();
                return;
            }

            OnAdvanceRequested?.Invoke();

            if (advanceButton && advanceButton.Hiding)
            {
                return;
            }

            if (typeWriter && typeWriter.IsTyping)
            {
                if (dialogueSettings != null && dialogueSettings.typingSkipAllowed)
                {
                    advanceButton?.SetHighlight();
                    typeWriter.Skip();
                }
                return;
            }

            advanceButton?.SetNormal();
            _onNext?.Invoke();
        }
        
        private string ResolveName(string characterID)
        {
            if (string.IsNullOrEmpty(characterID))
            {
                return string.Empty;
            }

            var row = FindCharacterRow(characterID);

            if (row == null)
            {
                return characterID;
            }

            return string.IsNullOrEmpty(row.displayName)
                ? characterID
                : row.displayName;
        }

        private string ResolvePortraitKey(string characterID, string portraitKey)
        {
            if (!string.IsNullOrEmpty(portraitKey))
            {
                return portraitKey;
            }

            var row = FindCharacterRow(characterID);

            if (row == null)
            {
                return string.Empty;
            }

            return row.defaultPortraitKey ?? string.Empty;
        }

        private CharacterDataTableRow FindCharacterRow(string characterID)
        {
            if (_characterTable == null)
            {
                return null;
            }

            return _characterTable.Find<CharacterDataTableRow>(characterID);
        }
    }
}
