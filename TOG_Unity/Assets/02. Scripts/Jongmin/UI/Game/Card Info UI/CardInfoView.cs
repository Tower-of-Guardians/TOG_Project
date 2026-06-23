using System;
using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class CardInfoView : ImageView
    {
        [SerializeField] private Toggle layerToggle;
        [SerializeField] private Button closeButton;

        public event Action<bool> OnToggleValueChanged;

        public void Bind(CardInfoSystem system)
        {
            layerToggle.onValueChanged.AddListener(ToggleChange);
            closeButton.onClick.AddListener(system.CloseView);
        }

        public void Show()
        {
            ToggleChange(false);

            CanvasGroup.Show();
        }

        public void Hide()
        {
            CanvasGroup.Hide();
        }
        
        public void ToggleActiveToggle(bool isActive)
        {
            layerToggle.gameObject.SetActive(isActive);
        }

        private void ToggleChange(bool isOn)
        {
            OnToggleValueChanged?.Invoke(isOn);
        }
    }
}