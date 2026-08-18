using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class FieldEventSystem : MonoBehaviour, IDropHandler
    {
        private FieldSystem _fieldSystem;
        private CardDropSystem _dropSystem;
        private CardContainer _container;
        private Vector2 _lastPointerPosition;
        
        public event Action<Card, FieldType> RequestOnBeginDrag;
        public event Action<Card, FieldType, Vector2> RequestSwapInSameField;
        public event Action<bool, FieldType> RequestOnEndDrag;
        public event Action<FieldType> RequestMoveHoverCardToOpposite;
        
        public void Construct(FieldSystem fieldSystem, CardDropSystem dropSystem, CardContainer container)
        {
            _fieldSystem = fieldSystem;
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

        public void HandleOnBeginDrag(Card card, PointerEventData eventData)
        {
            var fieldType = GetFieldType(card);
            
            RequestOnBeginDrag?.Invoke(card, fieldType);
        }

        public void HandleOnDrag(Card card, PointerEventData eventData)
        {
            _lastPointerPosition = eventData.position;

            if (_fieldSystem.HoverCard == null)
            {
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                RequestOnEndDrag?.Invoke(false, GetFieldType(card));
                return;
            }
            
            MoveHoverCardToMousePosition(eventData.position);

            if (!TryGetFieldCard(out var fieldCard))
            {
                return;
            }

            if (_container.IsExist(fieldCard))
            {
                RequestSwapInSameField?.Invoke(fieldCard, GetFieldType(fieldCard), eventData.position);
            }
        }

        public void HandleOnEndDrag(Card card, PointerEventData eventData)
        {
            _lastPointerPosition = eventData.position;

            if (_fieldSystem.HoverCard == null)
            {
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                RequestOnEndDrag?.Invoke(false, GetFieldType(card));
                return;
            }

            var fieldType = GetFieldType(card);
            
            var success = TryInvokeDropHandler(eventData.position);
            RequestOnEndDrag?.Invoke(success, fieldType);
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            var droppedObject = eventData.pointerDrag;
            if (droppedObject == null)
            {
                return;
            }
            
            var card = droppedObject.GetComponent<Card>();
            if (card == null || card.CardType != CardType.Hand)
            {
                return;
            }
            
            _dropSystem.OnDroppedHandToField(card, _fieldSystem.FieldType);
        }

        public bool TryMoveHoverCardToOppositeField()
        {
            var hit = CheckField(_lastPointerPosition, out _);
            if (hit == null)
            {
                return false;
            }
            
            var fieldEventSystem = CardDragRaycastResolver.GetComponentInParent<FieldEventSystem>(hit.Value);
            if (fieldEventSystem != null && fieldEventSystem != this)
            {
                RequestMoveHoverCardToOpposite?.Invoke(_fieldSystem.FieldType);
                return true;
            }
            
            var card = CardDragRaycastResolver.GetCard(hit.Value);

            if (card == null)
                return false;

            if (card.CardType is not (CardType.AtkField or CardType.DefField))
                return false;

            var targetFieldType = GetFieldType(card);

            if (targetFieldType == _fieldSystem.FieldType)
                return false;

            RequestMoveHoverCardToOpposite?.Invoke(_fieldSystem.FieldType);
            _fieldSystem.UpdateFieldStatus();
            return true;
        }

        private FieldType GetFieldType(Card card)
        {
            return card.CardType switch
            {
                CardType.AtkField => FieldType.Attack,
                CardType.DefField => FieldType.Defense
            };
        }

        private void MoveHoverCardToMousePosition(Vector2 position)
        {
            _fieldSystem.HoverCard.transform.position = position;
        }

        private RaycastResult? CheckField(Vector2 position, out PointerEventData eventData)
        {
            var rayHits = CardDragRaycastResolver.Raycast(
                position,
                _fieldSystem.HoverCard.gameObject,
                out eventData
            );

            foreach (var hit in rayHits)
            {
                var fieldEventSystem = CardDragRaycastResolver.GetComponentInParent<FieldEventSystem>(hit);
                if (fieldEventSystem != null && fieldEventSystem != this)
                {
                    return hit;
                }
                
                var card = CardDragRaycastResolver.GetCard(hit);
                if (card != null && _fieldSystem.HoverCard != card)
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

        private bool TryGetFieldCard(out Card card)
        {
            var cardHit = CheckField(_lastPointerPosition, out _);
            if (cardHit == null)
            {
                card = null;
                return false;
            }

            card = CardDragRaycastResolver.GetCard(cardHit.Value);
            if (card == null || card.CardType is not (CardType.AtkField or CardType.DefField))
            {
                card = null;
                return false;
            }

            return true;
        }

        private bool TryInvokeDropHandler(Vector2 position)
        {
            var handHit = CheckField(position, out var eventData);
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
    }
}
