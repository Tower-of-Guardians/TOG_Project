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
            OnPointerEntered?.Invoke(card);
        }

        private void HandleOnPointerExit(Card card, PointerEventData eventData)
        {
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
            if (!EnsureHoverCard(card))
            {
                OnDragCanceled?.Invoke();
                return;
            }

            card?.DOKill();
            RequestBeginDrag?.Invoke();
        }

        private void HandleOnDrag(Card card, PointerEventData eventData)
        {
            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                OnDragCanceled?.Invoke();
                return;
            }

            if (!EnsureHoverCard(card))
            {
                OnDragCanceled?.Invoke();
                return;
            }

            _handSystem.HoverCard.transform.position = eventData.position;

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
            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                OnDragCanceled?.Invoke();
                return;
            }

            if (!EnsureHoverCard(card))
            {
                OnDragCanceled?.Invoke();
                return;
            }

            var hit = CheckField(eventData.position, out var pointerData);
            var dropHandler = TryGetDropArea(hit, out var dropHandlerObject);
            if (dropHandler != null)
            {
                ExecuteEvents.Execute(dropHandlerObject, pointerData, ExecuteEvents.dropHandler);
            }
            
            RequestEndDrag?.Invoke();
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
