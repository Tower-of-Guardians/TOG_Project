using System;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    [ManagedEffect("UI", "Tooltip", 0)]
    public class TooltipEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Show/Hide")]
        [SerializeField] private float enableAlpha = 1f;
        [SerializeField] private float disableAlpha = 0f;
        [SerializeField] private float fadeDuration = 0.3f;

        public Tween PlayShowTooltipEffect(CanvasGroup canvasGroup)
        {
            Canvas.ForceUpdateCanvases();
            
            canvasGroup.alpha = disableAlpha;
            return canvasGroup.DOFade(enableAlpha, fadeDuration).SetUpdate(true);
        }

        public Tween PlayHideTooltipEffect(CanvasGroup canvasGroup, Action callback = null)
        {
            return canvasGroup.DOFade(disableAlpha, fadeDuration)
                .SetUpdate(true)
                .OnComplete(()=> callback?.Invoke());
        }
    }
}