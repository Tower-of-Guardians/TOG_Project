using System;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    [ManagedEffect("UI", "Craftman", 100)]
    public class CraftmanVisualEffect : MonoBehaviour
    {
        [BigHeader("Fade")]
        [SerializeField] private float activeFade = 1f;
        [SerializeField] private float deactiveFade = 0f;
        [SerializeField] private float fadeDuration = 1f;
        
        private Tween _toggleTween;

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
    }
}