using JxModule;
using TMPro;
using UnityEngine;

namespace Jongmin
{
    public class ShopView : ViewBase
    {
        [BigHeader("UI")]
        [SerializeField] private ButtonView saleButton;

        [Space(30f), BigHeader("Effect")]
        [SerializeField] private ShopVisualEffect shopVisualEffect;
        
        private MerchantDomain _domain;
        
        public void Bind(MerchantDomain merchantDomain)
        {
            _domain = merchantDomain;
            saleButton.AddListener(_domain.HandleOnClickedSale);
        }

        public void Show()
        {
            shopVisualEffect.PlayShowEffect(CanvasGroup);
        }

        public void Hide()
        {
            shopVisualEffect.PlayHideEffect(CanvasGroup);
        }

        public void SetSaleButtonState(bool canEnter)
        {
            saleButton.Button.interactable = canEnter;
            saleButton.Label.text = canEnter ? "카드 판매" : "<color=red>카드 판매</color>";
        }

        public void ReleaseEvents()
        {
            if (saleButton != null)
            {
                saleButton.RemoveListener(_domain.HandleOnClickedSale);
            }
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}