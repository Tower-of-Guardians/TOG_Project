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
            if (_handSystem.HoverCard == null)
            {
                OnDragCanceled?.Invoke();
                return;
            }

            card?.DOKill();
            RequestBeginDrag?.Invoke();
        }

        private void HandleOnDrag(Card card, PointerEventData eventData)
        {
            if (_handSystem.HoverCard == null || !_container.IsExist(card))
            {
                OnDragCanceled?.Invoke();
                return;
            }

            _handSystem.HoverCard.transform.position = eventData.position;

            var swapTargetCard = TryGetCard();
            if (swapTargetCard != null)
            {
                RequestSwapInSameField?.Invoke(swapTargetCard, eventData.position);
            }
            else
            {
                var dropHandler = TryGetDropArea(out _);
                var canDrop = dropHandler != null;

                RequestChangeDropState?.Invoke(canDrop);
            }
        }

        private void HandleOnEndDrag(Card card, PointerEventData eventData)
        {
            if (_handSystem.HoverCard == null || !_container.IsExist(card))
            {
                OnDragCanceled?.Invoke();
                return;
            }

            var hit = CheckField(out var pointerData);
            var dropHandler = hit?.gameObject.GetComponent<IDropHandler>();
            if (dropHandler != null)
            {
                ExecuteEvents.Execute(hit?.gameObject, pointerData, ExecuteEvents.dropHandler);
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

        private Card TryGetCard()
        {
            var hit = CheckField(out _);
            return hit?.gameObject.GetComponent<Card>();
        }

        private IDropHandler TryGetDropArea(out PointerEventData eventData)
        {
            var hit = CheckField(out eventData);
            return hit?.gameObject.GetComponent<IDropHandler>();
        }

        private RaycastResult? CheckField(out PointerEventData eventData)
        {
            eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
                pointerDrag = _handSystem.HoverCard.gameObject
            };

            var rayHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, rayHits);

            foreach (var hit in rayHits)
            {
                var card = hit.gameObject.GetComponent<Card>();
                if (card != null && _container.IsExist(card) && _handSystem.HoverCard != card)
                {
                    return hit;
                }
                
                var fieldHandler = hit.gameObject.GetComponent<FieldEventSystem>();
                if(fieldHandler != null)
                {
                    return hit;
                }

                var discardEventSystem = hit.gameObject.GetComponent<DiscardEventSystem>();
                if (discardEventSystem != null)
                {
                    return hit;
                }
            }

            return null;
        }
    }
}