using System;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class MerchantView : ViewBase
    {
        [SerializeField] private ButtonView exitButton;
        
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
        }

        public void ReleaseEvents()
        {
            if (exitButton != null && _domain != null)
            {
                exitButton.RemoveListener(_domain.CloseView);
            }
            _domain = null;
        }

        public void Show()
        {
            merchantVisualEffect.PlayShowEffect(CanvasGroup, CanvasGroup.Show);
        }

        public void Hide(Action onClosed = null)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            merchantVisualEffect.PlayHideEffect(CanvasGroup, () =>
            {
                CanvasGroup.Hide();
                onClosed?.Invoke();
            });
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}

