using System;
using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    [ManagedEffect("UI", "Dialogue", 4)]
    public class DialogueEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Translation")]
        [SerializeField] private float yTranslationOffset = 1080f;
        [SerializeField] private float translationDuration = 0.5f;

        private Tween _toggleTween;

        public Tween PlayShowEffect(Image backgroundImage,
                                    RectTransform dialoguePanelRect, 
                                    Vector2 dialoguePanelOriginAnchoredPosition, 
                                    Tween subTween)
        {
            _toggleTween?.Kill();
            
            var sequence = DOTween.Sequence()
                .Join(backgroundImage.DOFade(0.8f, translationDuration))
                .Join(subTween)
                .Append(dialoguePanelRect.DOAnchorPosY(dialoguePanelOriginAnchoredPosition.y + yTranslationOffset, translationDuration))
                .AppendInterval(0.5f);

            _toggleTween = sequence;
            return _toggleTween;
        }

        public Tween PlayHideEffect(Image backgroundImage,
                                    RectTransform dialoguePanelRect, 
                                    Vector2 dialoguePanelOriginAnchoredPosition, 
                                    Tween subTween,
                                    Action callback = null)
        {
            _toggleTween?.Kill();
            
            var sequence = DOTween.Sequence()
                .Join(dialoguePanelRect.DOAnchorPosY(dialoguePanelOriginAnchoredPosition.y, translationDuration))
                .Append(subTween)
                .Join(backgroundImage.DOFade(0f, translationDuration))
                .OnComplete(() => callback?.Invoke());
            
            _toggleTween = sequence;
            return _toggleTween;
        }
    }
}
