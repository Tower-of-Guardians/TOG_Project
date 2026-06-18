using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Jongmin
{
    public enum PreviewLayoutMode
    {
        None,
        Insert,
        Swap
    }

    public class DiscardCardLayout
    {
        private readonly DiscardSystem _system;
        private readonly DiscardUIDesigner _designer;
        private readonly CardContainer _container;
        private readonly PreviewCard _previewCard;

        public DiscardCardLayout(DiscardSystem system, DiscardUIDesigner designer, CardContainer container, PreviewCard previewCard)
        {
            _system = system;
            _designer = designer;
            _container = container;
            _previewCard = previewCard;
        }

        public void UpdateLayout(PreviewLayoutMode previewMode = PreviewLayoutMode.None, int previewIndex = -1, bool isAnime = true)
        {
            var cards = _container.Cards;
            var realCardCount = cards.Count;

            var hasPreview = previewMode != PreviewLayoutMode.None;

            var virtualCardCount = GetVirtualCardCount(previewMode, realCardCount);
            var resolvedPreviewIndex = GetPreviewIndex(previewMode, previewIndex, realCardCount);

            UpdateCards(
                cards,
                realCardCount,
                virtualCardCount,
                previewMode,
                resolvedPreviewIndex,
                isAnime
            );

            UpdatePreview(
                hasPreview,
                resolvedPreviewIndex,
                virtualCardCount
            );
        }

        private int GetVirtualCardCount(PreviewLayoutMode previewMode, int realCardCount)
        {
            return previewMode switch
            {
                PreviewLayoutMode.Insert => realCardCount + 1,
                PreviewLayoutMode.Swap => realCardCount,
                _ => realCardCount
            };
        }

        private int GetPreviewIndex(PreviewLayoutMode previewMode, int previewIndex, int realCardCount)
        {
            if (previewMode == PreviewLayoutMode.None)
            {
                return -1;
            }

            if (previewMode == PreviewLayoutMode.Insert)
            {
                if (previewIndex < 0)
                {
                    return realCardCount;
                }

                return Mathf.Clamp(previewIndex, 0, realCardCount);
            }

            if (previewMode == PreviewLayoutMode.Swap)
            {
                if (realCardCount <= 0)
                {
                    return -1;
                }

                if (previewIndex < 0)
                {
                    return 0;
                }

                return Mathf.Clamp(previewIndex, 0, realCardCount - 1);
            }

            return -1;
        }

        private void UpdateCards(IReadOnlyList<Card> cards,
                                 int realCardCount,
                                 int virtualCardCount,
                                 PreviewLayoutMode previewMode,
                                 int previewIndex,
                                 bool isAnime)
        {
            for (var i = 0; i < realCardCount; i++)
            {
                var card = cards[i];

                if (card == null)
                    continue;

                if (_system.HoverCard == card)
                    continue;

                var layoutIndex = GetCardLayoutIndex(
                    previewMode,
                    cardIndex: i,
                    previewIndex: previewIndex
                );

                var layoutPosition =
                    CardLayoutCalculator.CalculatedThrowCardPosition(
                        layoutIndex,
                        virtualCardCount,
                        _designer.Space
                    );

                ApplyCardPosition(card.RectTransform, layoutPosition, isAnime);
            }
        }

        private int GetCardLayoutIndex(PreviewLayoutMode previewMode, int cardIndex, int previewIndex)
        {
            return previewMode switch
            {
                PreviewLayoutMode.Insert when cardIndex >= previewIndex => cardIndex + 1,
                _ => cardIndex
            };
        }

        private void UpdatePreview(bool hasPreview, int previewIndex, int virtualCardCount)
        {
            if (!hasPreview)
            {
                return;
            }

            if (previewIndex < 0)
            {
                return;
            }

            var previewPosition =
                CardLayoutCalculator.CalculatedThrowCardPosition(
                    previewIndex,
                    virtualCardCount,
                    _designer.Space
                );

            var rectTransform = _previewCard.RectTransform;
            rectTransform.DOKill();
            rectTransform.anchoredPosition = previewPosition;
        }

        private void ApplyCardPosition(RectTransform rectTransform, Vector2 position, bool isAnime)
        {
            rectTransform.DOKill();

            if (isAnime)
            {
                rectTransform
                    .DOAnchorPos(position, _designer.AnimeDuration)
                    .SetEase(Ease.InOutSine);
            }
            else
            {
                rectTransform.anchoredPosition = position;
            }
        }
    }
}