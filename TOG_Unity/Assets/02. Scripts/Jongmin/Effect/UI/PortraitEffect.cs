using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    [ManagedEffect("UI", "Portrait", 5)]
    public class PortraitEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Translation")]
        [SerializeField] private float xTranslationOffset = 1920f;
        [SerializeField] private float translationDuration = 0.5f;

        public Tween PlayShowEffect(RectTransform playerSlotRect, 
                                    RectTransform npcSlotRect,
                                    Vector2 playerOriginAnchoredPosition, 
                                    Vector2 npcOriginAnchoredPosition)
        {
            return DOTween.Sequence()
                .Join(playerSlotRect.DOAnchorPosX(playerOriginAnchoredPosition.x + xTranslationOffset, translationDuration))
                .Join(npcSlotRect.DOAnchorPosX(npcOriginAnchoredPosition.x - xTranslationOffset, translationDuration));
        }

        public Tween PlayHideEffect(RectTransform playerSlotRect,
                                    RectTransform npcSlotRect,
                                    Vector2 playerOriginAnchoredPosition,
                                    Vector2 npcOriginAnchoredPosition)
        {
            return DOTween.Sequence()
                .Join(playerSlotRect.DOAnchorPosX(playerOriginAnchoredPosition.x, translationDuration))
                .Join(npcSlotRect.DOAnchorPosX(npcOriginAnchoredPosition.x, translationDuration));
        }
    }
}