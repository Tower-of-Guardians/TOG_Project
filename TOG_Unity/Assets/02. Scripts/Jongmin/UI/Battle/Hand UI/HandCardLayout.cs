using DG.Tweening;
using UnityEngine;

namespace Jongmin
{
    public class HandCardLayout
    {
        private readonly HandSystem _system;
        private readonly HandUIDesigner _designer;
        private readonly CardContainer _container;

        public HandCardLayout(HandSystem system, HandUIDesigner designer, CardContainer container)
        {
            _system = system;
            _designer = designer;
            _container = container;
        }

        public void UpdateLayout(bool isDragging = false)
        {
            var count = _container.Count;
            if (count == 0)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var card = _container.Get(i);

                if (isDragging && _system.HoverCard == card)
                {
                    continue;
                }
                
                var targetTransform 
                    = CardLayoutCalculator.CalculatedHandCardTransform(i, count, _designer.Radius, _designer.Angle, _designer.Depth);

                ApplyHoverEffect(targetTransform, card, i);
                ExecuteLayoutTween(targetTransform, card);
            }

            if (_system.HoverCard == null)
            {
                RebuildSiblingOrder();
            }
        }

        private void ApplyHoverEffect(CardLayoutData targetTransform, Card card, int cardIndex)
        {
            if (_system.HoverCard != null && !_container.IsExist(card))
            {
                _system.HoverCard = null;
                return;
            }

            if (_system.HoverCard == card)
            {
                targetTransform.scale = Vector3.one * _designer.Scale;
                targetTransform.rotation = Vector3.zero;
                
                card.transform.SetAsLastSibling();
            }
            else if (_system.HoverCard != null)
            {
                if (_container.TryGetIndex(_system.HoverCard, out var hoverIndex))
                {
                    var offset = cardIndex < hoverIndex ? -_designer.Strength : _designer.Strength;
                    targetTransform.position.x += offset;
                }
            }
        }

        private void ExecuteLayoutTween(CardLayoutData targetTransform, Card card)
        {
            if (card == null)
            {
                return;
            }
            
            card.transform.DOKill();

            card.transform.DOLocalMove(new Vector3(targetTransform.position.x, 
                                                   card == _system.HoverCard ? _designer.HoverY : targetTransform.position.y, 
                                                   targetTransform.position.z),
                                       _designer.AnimeSPD);

            card.transform.DOLocalRotate(targetTransform.rotation, _designer.AnimeSPD).SetEase(Ease.OutBack);
            card.transform.DOScale(targetTransform.scale, _designer.AnimeSPD).SetEase(Ease.OutBack);
        }

        private void RebuildSiblingOrder()
        {
            var count = _container.Count;
            if (count <= 0)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                _container.Cards[i].transform.SetSiblingIndex(i);
            }
        }
    }
}