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
            if (_fieldSystem.HoverCard == null)
            {
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
            if (_fieldSystem.HoverCard == null)
            {
                return;
            }

            var fieldType = GetFieldType(card);
            
            var success = TryInvokeDropHandler();
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
            var hit = CheckField(out _);
            if (hit == null)
            {
                return false;
            }
            
            var fieldEventSystem = hit.Value.gameObject.GetComponent<FieldEventSystem>();
            if (fieldEventSystem != null && fieldEventSystem != this)
            {
                RequestMoveHoverCardToOpposite?.Invoke(_fieldSystem.FieldType);
                return true;
            }
            
            var card = hit.Value.gameObject.GetComponent<Card>();

            if (card == null)
                return false;

            if (card.CardType is not (CardType.AtkField or CardType.DefField))
                return false;

            var targetFieldType = GetFieldType(card);

            if (targetFieldType == _fieldSystem.FieldType)
                return false;

            RequestMoveHoverCardToOpposite?.Invoke(_fieldSystem.FieldType);
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

        private RaycastResult? CheckField(out PointerEventData eventData)
        {
            eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
                pointerDrag = _fieldSystem.HoverCard.gameObject
            };

            var rayHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, rayHits);

            foreach (var hit in rayHits)
            {
                var fieldEventSystem = hit.gameObject.GetComponent<FieldEventSystem>();
                if (fieldEventSystem != null && fieldEventSystem != this)
                {
                    return hit;
                }
                
                var card = hit.gameObject.GetComponent<Card>();
                if (card != null && _fieldSystem.HoverCard != card)
                {
                    return hit;
                }
                
                var dropHandler = hit.gameObject.GetComponent<IDropHandler>();
                if (dropHandler != null)
                {
                    return hit;
                }
            }

            return null;
        }

        private bool TryGetFieldCard(out Card card)
        {
            var cardHit = CheckField(out _);
            if (cardHit == null)
            {
                card = null;
                return false;
            }

            card = cardHit.Value.gameObject.GetComponent<Card>();
            if (card == null || card.CardType is not (CardType.AtkField or CardType.DefField))
            {
                card = null;
                return false;
            }

            return true;
        }

        private bool TryInvokeDropHandler()
        {
            var handHit = CheckField(out var eventData);
            if (handHit == null)
            {
                return false;
            }
            
            var handEventSystem = handHit.Value.gameObject.GetComponent<HandEventSystem>();
            if (handEventSystem == null)
            {
                return false;
            }
            
            ExecuteEvents.Execute(handHit.Value.gameObject, eventData, ExecuteEvents.dropHandler);
            return true;
        }
    }
}