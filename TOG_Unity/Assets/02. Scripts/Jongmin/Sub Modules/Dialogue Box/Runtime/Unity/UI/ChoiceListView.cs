using System;
using System.Collections.Generic;
using JxModule;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JxDialogueBox
{
    public class ChoiceListView : MonoBehaviour
    {
        [BigHeader("UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform optionContainer;
        [SerializeField] private Button optionPrefab;

        private Action<int> _onChoose;
        private int _selectedIndex = -1;
        private readonly List<Button> _buttons = new();

        public int Count => _buttons.Count;

        public void Bind(Action<int> onChooseAction)
        {
            _onChoose = onChooseAction;
        }

        public void Show(ChoiceOption[] options)
        {
            canvasGroup?.Show();
            Clear();

            if (options == null)
            {
                return;
            }

            for (var i = 0; i < options.Length; i++)
            {
                var optionObj = Instantiate(optionPrefab, optionContainer);
                _buttons.Add(optionObj);
                
                var index = i;
                var tmp = optionObj.GetComponentInChildren<TMP_Text>();

                if (tmp)
                {
                    tmp.text = "· " + options[index].Text;
                }

                optionObj.onClick.AddListener(() => _onChoose?.Invoke(index));
                BindPointerEnter(optionObj);
            }

            ClearSelection();
        }

        public void Hide()
        {
            canvasGroup?.Hide();
            Clear();
        }

        public void SetSelected(int index)
        {
            if (_buttons.Count == 0)
            {
                _selectedIndex = -1;
                return;
            }

            _selectedIndex = Mathf.Clamp(index, 0, _buttons.Count - 1);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_buttons[_selectedIndex].gameObject);
            }
        }

        public void MoveSelection(int delta, bool wrap = true)
        {
            if (_buttons.Count == 0)
            {
                return;
            }

            var nextIndex = _selectedIndex < 0
                ? GetInitialKeyboardIndex(delta)
                : _selectedIndex + delta;

            if(wrap)
            {
                if (nextIndex < 0)
                {
                    nextIndex = _buttons.Count - 1;
                }

                if (nextIndex >= _buttons.Count)
                {
                    nextIndex = 0;
                }
            }
            else
            {
                nextIndex = Mathf.Clamp(nextIndex, 0, _buttons.Count - 1);
            }

            SetSelected(nextIndex);
        }

        public void ConfirmSelection()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _buttons.Count)
            {
                return;
            }

            Choose(_selectedIndex);
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= _buttons.Count)
            {
                return;
            }

            _onChoose?.Invoke(index);            
        }

        private void BindPointerEnter(Button option)
        {
            if (option == null)
            {
                return;
            }

            var trigger = option.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = option.gameObject.AddComponent<EventTrigger>();
            }

            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener(_ => ClearSelection());
            trigger.triggers.Add(entry);
        }

        private int GetInitialKeyboardIndex(int delta)
        {
            return delta < 0 ? _buttons.Count - 1 : 0;
        }

        private void ClearSelection()
        {
            _selectedIndex = -1;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void Clear()
        {
            if (optionContainer == null)
            {
                return;
            }
            
            optionContainer.DestroyChildren();
            _buttons.Clear();
            _selectedIndex = -1;
        }
    }
}
