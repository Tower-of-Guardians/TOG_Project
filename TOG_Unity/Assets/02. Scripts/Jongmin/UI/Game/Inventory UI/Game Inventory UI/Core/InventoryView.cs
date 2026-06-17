using DG.Tweening;
using JxModule;
using TMPro;
using UnityEngine;

namespace Jongmin
{
    public class InventoryView : ViewBase
    {
        [SerializeField] private ButtonView toggleButton;
        [SerializeField] private TMP_Text titleLabel;

        private Tween _toggleTween;

        public void Bind(InventorySystem system)
        {
            toggleButton.AddListener(system.ToggleView);
        }

        public void SetTitle(string titleText)
        {
            titleLabel.text = titleText;
        }

        public void Show()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(1f, 0.25f).OnComplete(CanvasGroup.Show);
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.25f).OnComplete(CanvasGroup.Hide);
        }
    }
}