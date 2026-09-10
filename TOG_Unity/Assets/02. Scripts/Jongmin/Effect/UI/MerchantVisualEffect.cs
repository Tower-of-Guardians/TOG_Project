using System;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    [ManagedEffect("UI", "Merchant", 102)]
    public class MerchantVisualEffect : MonoBehaviour
    {
        [BigHeader("Fade")]
        [SerializeField] private float activeFade = 1f;
        [SerializeField] private float deactiveFade = 0f;
        [SerializeField] private float fadeDuration = 1f;
        
        private Tween _toggleTween;
        private Tween _cancelButtonTween;
        private Tween _sellButtonTween;

        public void PlayShowEffect(CanvasGroup canvasGroup, Action callback = null)
        {
            _toggleTween?.Kill();
            _toggleTween = canvasGroup.DOFade(activeFade, fadeDuration).OnComplete(() =>
            {
                callback?.Invoke();
                _toggleTween = null;
            });
        }

        public void PlayHideEffect(CanvasGroup canvasGroup, Action callback = null)
        {
            _toggleTween?.Kill();
            _toggleTween = canvasGroup.DOFade(deactiveFade, fadeDuration).OnComplete(() =>
            {
                callback?.Invoke();
                _toggleTween = null;
            });
        }

        public void PlayShowCancelButtonEffect(CanvasGroup cancelButtonGroup, Action callback = null)
        {
            _cancelButtonTween?.Kill();
            _cancelButtonTween = cancelButtonGroup.DOFade(activeFade, fadeDuration).OnComplete(() =>
            {
                callback?.Invoke();
                _cancelButtonTween = null;
            });
        }

        public void PlayHideCancelButtonEffect(CanvasGroup cancelButtonGroup, Action callback = null)
        {
            _cancelButtonTween?.Kill();
            _cancelButtonTween = cancelButtonGroup.DOFade(deactiveFade, fadeDuration).OnComplete(() =>
            {
                callback?.Invoke();
                _cancelButtonTween = null;
            });
        }

        public void PlayShowSellButtonEffect(CanvasGroup sellButtonGroup, Action callback = null)
        {
            _sellButtonTween?.Kill();
            _sellButtonTween = sellButtonGroup.DOFade(activeFade, fadeDuration).OnComplete(() =>
            {
                callback?.Invoke();
                _sellButtonTween = null;
            });
        }

        public void PlayHideSellButtonEffect(CanvasGroup sellButtonGroup, Action callback = null)
        {
            _sellButtonTween?.Kill();
            _sellButtonTween = sellButtonGroup.DOFade(deactiveFade, fadeDuration).OnComplete(() =>
            {
                callback?.Invoke();
                _sellButtonTween = null;
            });
        }
    }
}
