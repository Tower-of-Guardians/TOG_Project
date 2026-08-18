using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class HandEventSystem : MonoBehaviour, IDropHandler
    {
        private HandSystem _handSystem;
        private CardDropSystem _dropSystem;
        private CardContainer _container;
        private Vector2 _dragPointerOffset;
        private Vector3 _dragStartScale = Vector3.one;
        private bool _isDragging;
        private bool _isDragCanceled;

        public event Action<Card> OnPointerEntered;
        public event Action OnPointerExited;
        public event Action OnDragCanceled;
        public event Action RequestBeginDrag;
        public event Action<Card, Vector2> RequestSwapInSameField;
        public event Action<bool> RequestChangeDropState;
        public event Action RequestEndDrag;
        public event Action<CardData> OnPointerClicked;

        public void Construct(HandSystem handSystem, CardDropSystem dropSystem, CardContainer container)
        {
            _handSystem = handSystem;
            _dropSystem = dropSystem;
            _container = container;
        }
        
        public void Subscribe(Card card)
        {
            card.Pointer.OnPointerEntered += HandleOnPointerEnter;
            card.Pointer.OnPointerExited += HandleOnPointerExit;
            card.Pointer.OnPointerClicked += HandleOnPointerClick;
            card.Pointer.OnBeginDragged += HandleOnBeginDrag;
            card.Pointer.OnDragged += HandleOnDrag;
            card.Pointer.OnEndDragged += HandleOnEndDrag;
        }

        public void Unsubscribe(Card card)
        {
            card.Pointer.OnPointerEntered -= HandleOnPointerEnter;
            card.Pointer.OnPointerExited -= HandleOnPointerExit;
            card.Pointer.OnPointerClicked -= HandleOnPointerClick;
            card.Pointer.OnBeginDragged -= HandleOnBeginDrag;
            card.Pointer.OnDragged -= HandleOnDrag;
            card.Pointer.OnEndDragged -= HandleOnEndDrag;
        }

        private void HandleOnPointerEnter(Card card, PointerEventData eventData)
        {
            if (_isDragging)
            {
                return;
            }

            OnPointerEntered?.Invoke(card);
        }

        private void HandleOnPointerExit(Card card, PointerEventData eventData)
        {
            if (_isDragging)
            {
                return;
            }

            OnPointerExited?.Invoke();
        }

        private void HandleOnPointerClick(Card card, PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                return;
            }
            
            if (_handSystem.HoverCard == null || !_container.IsExist(card))
            {
                return;
            }
            
            OnPointerClicked?.Invoke(card.CardData);
        }

        private void HandleOnBeginDrag(Card card, PointerEventData eventData)
        {
            _isDragging = true;
            _isDragCanceled = false;

            if (!EnsureHoverCard(card))
            {
                CancelDrag();
                return;
            }

            card?.DOKill();
            card.RectTransform.localRotation = Quaternion.identity;
            RequestBeginDrag?.Invoke();
            CacheDragPointerOffset(eventData);
        }

        private void HandleOnDrag(Card card, PointerEventData eventData)
        {
            if (_isDragCanceled)
            {
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                CancelDrag();
                return;
            }

            if (!EnsureHoverCard(card))
            {
                CancelDrag();
                return;
            }

            MoveHoverCardToMousePosition(eventData);

            var swapTargetCard = TryGetCard(eventData.position);
            if (swapTargetCard != null)
            {
                RequestSwapInSameField?.Invoke(swapTargetCard, eventData.position);
            }
            else
            {
                var dropHandler = TryGetDropArea(eventData.position, out _, out _);
                var canDrop = dropHandler != null;

                RequestChangeDropState?.Invoke(canDrop);
            }
        }

        private void HandleOnEndDrag(Card card, PointerEventData eventData)
        {
            if (_isDragCanceled)
            {
                _isDragging = false;
                _isDragCanceled = false;
                _dragPointerOffset = Vector2.zero;
                _dragStartScale = Vector3.one;
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                CancelDrag();
                _isDragging = false;
                _isDragCanceled = false;
                _dragPointerOffset = Vector2.zero;
                _dragStartScale = Vector3.one;
                return;
            }

            if (!EnsureHoverCard(card))
            {
                CancelDrag();
                _isDragging = false;
                _isDragCanceled = false;
                _dragPointerOffset = Vector2.zero;
                _dragStartScale = Vector3.one;
                return;
            }

            var hit = CheckField(eventData.position, out var pointerData);
            var dropHandler = TryGetDropArea(hit, out var dropHandlerObject);
            if (dropHandler != null)
            {
                ExecuteEvents.Execute(dropHandlerObject, pointerData, ExecuteEvents.dropHandler);
            }
            
            RequestEndDrag?.Invoke();
            _isDragging = false;
            _isDragCanceled = false;
            _dragPointerOffset = Vector2.zero;
            _dragStartScale = Vector3.one;
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

            switch (card.CardType)
            {
                case CardType.Discard:
                    _dropSystem.OnDroppedDiscardToHand(card);
                    break;
                
                case CardType.AtkField:
                case CardType.DefField:
                    _dropSystem.OnDroppedFieldToHand(card);
                    break;
            }
        }

        private bool EnsureHoverCard(Card card)
        {
            if (card == null || !_container.IsExist(card))
            {
                return false;
            }

            if (_handSystem.HoverCard == null)
            {
                _handSystem.HoverCard = card;
            }
            
            return true;
        }

        private void MoveHoverCardToMousePosition(PointerEventData eventData)
        {
            var hoverCard = _handSystem.HoverCard;
            if (hoverCard == null)
            {
                return;
            }

            if (!CardDragRaycastResolver.TrySetAnchoredPositionFromScreenPoint(hoverCard.RectTransform,
                                                                               eventData.position,
                                                                               eventData.pressEventCamera,
                                                                               GetScaledDragPointerOffset(hoverCard)))
            {
                hoverCard.transform.position = eventData.position;
            }

            hoverCard.RectTransform.localRotation = Quaternion.identity;
        }

        private void CacheDragPointerOffset(PointerEventData eventData)
        {
            var hoverCard = _handSystem.HoverCard;
            if (hoverCard == null)
            {
                _dragPointerOffset = Vector2.zero;
                _dragStartScale = Vector3.one;
                return;
            }

            _dragStartScale = hoverCard.RectTransform.localScale;

            if (!CardDragRaycastResolver.TryGetPointerOffset(hoverCard.RectTransform,
                                                             eventData.position,
                                                             eventData.pressEventCamera,
                                                             out _dragPointerOffset))
            {
                _dragPointerOffset = Vector2.zero;
            }
        }

        private Vector2 GetScaledDragPointerOffset(Card hoverCard)
        {
            if (hoverCard == null)
            {
                return _dragPointerOffset;
            }

            var currentScale = hoverCard.RectTransform.localScale;
            var ratioX = Mathf.Approximately(_dragStartScale.x, 0f) ? 1f : currentScale.x / _dragStartScale.x;
            var ratioY = Mathf.Approximately(_dragStartScale.y, 0f) ? 1f : currentScale.y / _dragStartScale.y;

            return new Vector2(_dragPointerOffset.x * ratioX, _dragPointerOffset.y * ratioY);
        }

        private void CancelDrag()
        {
            if (_isDragCanceled)
            {
                return;
            }

            _isDragCanceled = true;
            _handSystem.HoverCard?.Pointer.CancelDrag();
            OnDragCanceled?.Invoke();
        }

        private Card TryGetCard(Vector2 position)
        {
            var hit = CheckField(position, out _);
            return hit == null ? null : CardDragRaycastResolver.GetCard(hit.Value);
        }

        private IDropHandler TryGetDropArea(Vector2 position,
                                            out PointerEventData eventData,
                                            out GameObject dropHandlerObject)
        {
            var hit = CheckField(position, out eventData);
            return TryGetDropArea(hit, out dropHandlerObject);
        }

        private IDropHandler TryGetDropArea(RaycastResult? hit, out GameObject dropHandlerObject)
        {
            dropHandlerObject = null;
            if (hit == null)
            {
                return null;
            }

            var fieldHandler = CardDragRaycastResolver.GetComponentInParent<FieldEventSystem>(hit.Value);
            if (fieldHandler != null)
            {
                dropHandlerObject = fieldHandler.gameObject;
                return fieldHandler;
            }

            var discardHandler = CardDragRaycastResolver.GetComponentInParent<DiscardEventSystem>(hit.Value);
            if (discardHandler != null)
            {
                dropHandlerObject = discardHandler.gameObject;
                return discardHandler;
            }

            return null;
        }

        private RaycastResult? CheckField(Vector2 position, out PointerEventData eventData)
        {
            var rayHits = CardDragRaycastResolver.Raycast(
                position,
                _handSystem.HoverCard.gameObject,
                out eventData
            );

            foreach (var hit in rayHits)
            {
                var card = CardDragRaycastResolver.GetCard(hit);
                if (card != null && _container.IsExist(card) && _handSystem.HoverCard != card)
                {
                    return hit;
                }
                
                var fieldHandler = CardDragRaycastResolver.GetComponentInParent<FieldEventSystem>(hit);
                if(fieldHandler != null)
                {
                    return hit;
                }

                var discardEventSystem = CardDragRaycastResolver.GetComponentInParent<DiscardEventSystem>(hit);
                if (discardEventSystem != null)
                {
                    return hit;
                }
            }

            return null;
        }
    }
}
