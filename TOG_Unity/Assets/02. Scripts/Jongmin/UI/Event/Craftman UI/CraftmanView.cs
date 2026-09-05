using JxModule;
using UnityEngine;
using System;

namespace Jongmin
{
    public class CraftmanView : ViewBase
    {
        [SerializeField] private ButtonView exitButton;

        [BigHeader("Effect")]
        [SerializeField] private CraftmanVisualEffect craftmanVisualEffect;

        private CraftmanDomain _domain;

        public void Bind(CraftmanDomain domain)
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
            craftmanVisualEffect.PlayShowEffect(CanvasGroup, CanvasGroup.Show);
        }

        public void Hide()
        {
            Hide(null);
        }

        public void Hide(Action onClosed)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            craftmanVisualEffect.PlayHideEffect(CanvasGroup, () =>
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
