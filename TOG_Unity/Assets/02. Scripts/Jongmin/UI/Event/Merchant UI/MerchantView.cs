using System;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class MerchantView : ViewBase
    {
        [SerializeField] private ButtonView exitButton;
        [SerializeField] private ButtonView cancelButton;
        [SerializeField] private ButtonView sellButton;
        
        [BigHeader("Effect")]
        [SerializeField] private MerchantVisualEffect merchantVisualEffect;

        private MerchantDomain _domain;

        public void Bind(MerchantDomain domain)
        {
            ReleaseEvents();
            _domain = domain;
            if (exitButton != null)
            {
                exitButton.AddListener(_domain.CloseView);
            }

            if (cancelButton != null)
            {
                cancelButton.AddListener(_domain.HandleOnCanceledSale);
            }

            if (sellButton != null)
            {
                sellButton.AddListener(_domain.HandleOnClickedSell);
            }
        }

        public void ReleaseEvents()
        {
            if (exitButton != null && _domain != null)
            {
                exitButton.RemoveListener(_domain.CloseView);
            }

            if (cancelButton != null && _domain != null)
            {
                cancelButton.RemoveListener(_domain.HandleOnCanceledSale);
            }

            if (sellButton != null && _domain != null)
            {
                sellButton.RemoveListener(_domain.HandleOnClickedSell);
            }

            _domain = null;
        }

        public void Show()
        {
            HideSaleButtonsImmediate();
            exitButton.CanvasGroup.Show();
            merchantVisualEffect.PlayShowEffect(CanvasGroup, CanvasGroup.Show);
        }

        public void Hide(Action onClosed = null)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            HideSaleButtonsImmediate();
            merchantVisualEffect.PlayHideEffect(CanvasGroup, () =>
            {
                CanvasGroup.Hide();
                onClosed?.Invoke();
            });
        }

        public void ShowSaleButtons()
        {
            exitButton.CanvasGroup.Hide();
            cancelButton.CanvasGroup.interactable = true;
            cancelButton.CanvasGroup.blocksRaycasts = true;
            SetSellButtonInteractable(false);

            merchantVisualEffect.PlayShowCancelButtonEffect(cancelButton.CanvasGroup);
            merchantVisualEffect.PlayShowSellButtonEffect(sellButton.CanvasGroup);
        }

        public void HideSaleButtons(Action onClosed = null)
        {
            exitButton.CanvasGroup.Show();
            cancelButton.CanvasGroup.interactable = false;
            cancelButton.CanvasGroup.blocksRaycasts = false;
            sellButton.CanvasGroup.interactable = false;
            sellButton.CanvasGroup.blocksRaycasts = false;

            merchantVisualEffect.PlayHideCancelButtonEffect(cancelButton.CanvasGroup, () =>
            {
                cancelButton.CanvasGroup.Hide();
                onClosed?.Invoke();
            });
            merchantVisualEffect.PlayHideSellButtonEffect(sellButton.CanvasGroup, () =>
            {
                sellButton.CanvasGroup.Hide();
            });
        }

        public void SetSellButtonInteractable(bool isInteractable)
        {
            sellButton.Button.interactable = isInteractable;
            sellButton.CanvasGroup.interactable = isInteractable;
            sellButton.CanvasGroup.blocksRaycasts = isInteractable;
        }

        private void HideSaleButtonsImmediate()
        {
            cancelButton.CanvasGroup.Hide();
            sellButton.CanvasGroup.Hide();
            SetSellButtonInteractable(false);
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}
