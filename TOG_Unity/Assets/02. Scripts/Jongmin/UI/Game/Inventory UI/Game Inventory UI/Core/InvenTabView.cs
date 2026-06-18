using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class InvenTabView : ViewBase
    {
        [SerializeField] private TabButtonView cardTabButton;
        [SerializeField] private TabButtonView magicTabButton;

        private Tween _toggleTween;

        public void Bind(InventoryDomain domain)
        {
            domain.OnTabDeselected += cardTabButton.DeselectButton;
            domain.OnTabDeselected += magicTabButton.DeselectButton;
            
            cardTabButton.AddListener(domain.HandleOnClickedCardTab);
            magicTabButton.AddListener(domain.HandleOnClickedMagicTab);
        }

        public void Initialize()
        {
            cardTabButton.SelectButton();
        }

        public void Show()
        {
            _toggleTween?.Kill();
            _toggleTween = RectTransform.DOAnchorPosX(-100f, 0.25f);
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = RectTransform.DOAnchorPosX(120f, 0.25f);
        }
    }
}