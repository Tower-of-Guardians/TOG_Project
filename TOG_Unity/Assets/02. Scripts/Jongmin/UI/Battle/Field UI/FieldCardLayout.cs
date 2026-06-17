using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Jongmin
{
    public enum FieldPreviewMode
    {
        None,
        Insert,
        Swap,
    }
    
    public class FieldCardLayout
    {
        private readonly FieldSystem _system;
        private readonly FieldUIDesigner _designer;
        private readonly CardContainer _container;
        private readonly PreviewCard _previewCard;

        public FieldCardLayout(FieldSystem system, FieldUIDesigner designer, CardContainer container, PreviewCard previewCard)
        {
            _system = system;
            _designer = designer;
            _container = container;
            _previewCard = previewCard;
        }

        public void UpdateLayout(FieldPreviewMode previewMode = FieldPreviewMode.None, int previewIndex = -1, bool isAnime = true)
        {
            var cards = _container.Cards;
            var cardCount = cards.Count;

            UpdateCards(cards, cardCount, isAnime);
            UpdatePreview(previewMode, previewIndex, cardCount);
        }

        private void UpdateCards(IReadOnlyList<Card> cards, int cardCount, bool isAnime)
        {
            for (var i = 0; i < cardCount; i++)
            {
                var card = cards[i];
                if (card == null)
                {
                    continue;
                }

                if (card == _system.HoverCard)
                {
                    continue;
                }
                
                var layoutPosition =
                    CardLayoutCalculator.CalculatedFieldCardPosition(
                        i, 
                        _designer.ATKLimit, 
                        _designer.Space
                    );

                card.transform.DOKill();

                if (isAnime)
                {
                    card.RectTransform.DOAnchorPos(layoutPosition, _designer.AnimeDuration).SetEase(Ease.InOutSine);
                }
                else
                {
                    card.RectTransform.anchoredPosition = layoutPosition;
                }
            }
        }

        private void UpdatePreview(FieldPreviewMode previewMode, int previewIndex, int cardCount)
        {
            if (previewMode == FieldPreviewMode.None)
            {
                return;
            }
            
            var resolvedIndex = ResolvePreviewIndex(previewMode, previewIndex, cardCount);
            if (resolvedIndex < 0 || resolvedIndex >= _designer.ATKLimit)
            {
                return;
            }
            
            var previewPosition =
                CardLayoutCalculator.CalculatedFieldCardPosition(
                    resolvedIndex,
                    _designer.ATKLimit,
                    _designer.Space
                );
            
            var rectTransform = _previewCard.RectTransform;
            rectTransform.DOKill();
            rectTransform.anchoredPosition = previewPosition;
        }
        
        private int ResolvePreviewIndex(FieldPreviewMode previewMode, int previewIndex, int cardCount)
        {
            return previewMode switch
            {
                FieldPreviewMode.Insert => cardCount,
                FieldPreviewMode.Swap => Mathf.Clamp(previewIndex, 0, Mathf.Max(0, cardCount - 1)),
                _ => -1
            };
        }
    }
}