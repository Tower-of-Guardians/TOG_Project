using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    [ManagedEffect("UI", "Shop", 103)]
    public class ShopVisualEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [SerializeField] private float activeAlpha = 1f;
        [SerializeField] private float inactiveAlpha = 0f;
        [SerializeField] private float fadeDuration = 0.5f;
        
        private Tween _fadeTween;
        
        public void PlayShowEffect(CanvasGroup shopGroup)
        {
            _fadeTween?.Kill();
            _fadeTween = shopGroup.DOFade(activeAlpha, fadeDuration);
            shopGroup.interactable = true;
            shopGroup.blocksRaycasts = true;
        }
        
        public void PlayHideEffect(CanvasGroup shopGroup)
        {
            _fadeTween?.Kill();
            _fadeTween = shopGroup.DOFade(inactiveAlpha, fadeDuration);
            shopGroup.interactable = false;
            shopGroup.blocksRaycasts = false;
        }
    }
}