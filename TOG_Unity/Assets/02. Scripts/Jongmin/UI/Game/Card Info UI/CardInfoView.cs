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

        private Tweener _toggleTween;

        public event Action<bool> OnToggleValueChanged;

        public void Bind(CardInfoSystem system)
        {
            layerToggle.onValueChanged.AddListener(ToggleChange);
            closeButton.onClick.AddListener(system.CloseView);
        }

        public void Show()
        {
            ToggleChange(false);
            
            _toggleTween?.Kill();
            
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;

            _toggleTween = CanvasGroup.DOFade(1f, 0.25f);
        }

        public void Hide()
        {
            layerToggle.isOn = false;
            
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.25f).OnComplete(CanvasGroup.Hide);
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