using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class DiscardEventSystem : MonoBehaviour, IDropHandler
    {
        private DiscardSystem _discardSystem;
        private CardDropSystem _dropSystem;
        private CardContainer _container;
        private Vector2 _dragPointerOffset;
        private bool _isDragCanceled;
        
        public event Action<Card> RequestOnBeginDrag;
        public event Action<Card, Vector2> RequestSwapInSameField;
        public event Action<bool> RequestOnEndDrag;

        public void Construct(DiscardSystem discardSystem, CardDropSystem dropSystem, CardContainer container)
        {
            _discardSystem = discardSystem;
            _dropSystem = dropSystem;
            _container = container;
        }

        public void Subscribe(Card card)
        {
            card.Pointer.OnBeginDragged += HandleOnBeginDrag;
            card.Pointer.OnDragged += HandleOnDrag;
            card.Pointer.OnEndDragged += HandleOnEndDrag;
        }

        public void Unsubscribe(Card card)
        {
            card.Pointer.OnBeginDragged -= HandleOnBeginDrag;
            card.Pointer.OnDragged -= HandleOnDrag;
            card.Pointer.OnEndDragged -= HandleOnEndDrag;
        }

        private void HandleOnBeginDrag(Card card, PointerEventData eventData)
        {
            _isDragCanceled = false;
            RequestOnBeginDrag?.Invoke(card);
            CacheDragPointerOffset(eventData);
        }

        private void HandleOnDrag(Card card, PointerEventData eventData)
        {
            if (_isDragCanceled)
            {
                return;
            }

            if (_discardSystem.HoverCard == null)
            {
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                CancelDrag();
                return;
            }

            MoveHoverCardToMousePosition(eventData);

            if (TryGetCard(eventData.position, out var targetCard))
            {
                RequestSwapInSameField?.Invoke(targetCard, eventData.position);
            }
        }

        private void HandleOnEndDrag(Card card, PointerEventData eventData)
        {
            if (_isDragCanceled)
            {
                _isDragCanceled = false;
                _dragPointerOffset = Vector2.zero;
                return;
            }

            if (_discardSystem.HoverCard == null)
            {
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                CancelDrag();
                _isDragCanceled = false;
                _dragPointerOffset = Vector2.zero;
                return;
            }
              
            var success = TryInvokeDropHandler(eventData.position);
            RequestOnEndDrag?.Invoke(success);
            _isDragCanceled = false;
            _dragPointerOffset = Vector2.zero;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var droppedObject = eventData.pointerDrag;
            if (droppedObject == null)
            {
                return;
            }
            
            var card = droppedObject.GetComponent<Card>();
            if (card == null)
            {
                return;
            }

            if (card.Pointer.IsDragCanceled)
            {
                return;
            }

            if (card.CardType == CardType.Hand)
            {
                _dropSystem.OnDroppedHandToDiscard(card);    
            }
        }

        private void MoveHoverCardToMousePosition(PointerEventData eventData)
        {
            var hoverCard = _discardSystem.HoverCard;
            if (hoverCard == null)
            {
                return;
            }

            if (!CardDragRaycastResolver.TrySetAnchoredPositionFromScreenPoint(hoverCard.RectTransform,
                                                                               eventData.position,
                                                                               eventData.pressEventCamera,
                                                                               _dragPointerOffset))
            {
                hoverCard.transform.position = eventData.position;
            }
        }

        private void CacheDragPointerOffset(PointerEventData eventData)
        {
            var hoverCard = _discardSystem.HoverCard;
            if (hoverCard == null)
            {
                _dragPointerOffset = Vector2.zero;
                return;
            }

            if (!CardDragRaycastResolver.TryGetPointerOffset(hoverCard.RectTransform,
                                                             eventData.position,
                                                             eventData.pressEventCamera,
                                                             out _dragPointerOffset))
            {
                _dragPointerOffset = Vector2.zero;
            }
        }

        private void CancelDrag()
        {
            if (_isDragCanceled)
            {
                return;
            }

            _isDragCanceled = true;
            _discardSystem.HoverCard?.Pointer.CancelDrag();
            RequestOnEndDrag?.Invoke(false);
        }

        private bool TryInvokeDropHandler(Vector2 position)
        {
            var handHit  = CheckField(position, out var eventData);
            if (handHit == null)
            {
                return false;
            }
            
            var handEventSystem = CardDragRaycastResolver.GetComponentInParent<HandEventSystem>(handHit.Value);
            if (handEventSystem == null)
            {
                return false;
            }
            
            ExecuteEvents.Execute(handEventSystem.gameObject, eventData, ExecuteEvents.dropHandler);
            return true;
        }

        private bool TryGetCard(Vector2 position, out Card card)
        {
            card = null;

            var cardHit = CheckField(position, out _);
            if (cardHit == null)
            {
                return false;
            }
            
            card = CardDragRaycastResolver.GetCard(cardHit.Value);
            if (card == null)
            {
                return false;
            }
            
            return true;
        }

        private RaycastResult? CheckField(Vector2 position, out PointerEventData eventData)
        {
            var rayHits = CardDragRaycastResolver.Raycast(
                position,
                _discardSystem.HoverCard.gameObject,
                out eventData
            );

            foreach (var hit in rayHits)
            {
                var card = CardDragRaycastResolver.GetCard(hit);
                if (card != null && _container.IsExist(card) && _discardSystem.HoverCard != card)
                {
                    return hit;
                }
                
                var handHandler = CardDragRaycastResolver.GetComponentInParent<HandEventSystem>(hit);
                if (handHandler != null)
                {
                    return hit;
                }
            }

            return null;
        }
    }
}
