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
        private Vector2 _dragPointerOffset;
        private FieldType _dragFieldType;
        private bool _isDragging;
        private bool _isDragCanceled;
        private bool _isEndDragRequested;
        private Action _pendingDropCommit;

        public FieldType FieldType => _fieldSystem.FieldType;
        
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
            if (_isDragging)
            {
                CompleteDrag(false);
            }

            _isDragging = true;
            _isDragCanceled = false;
            _isEndDragRequested = false;
            _pendingDropCommit = null;
            _dragFieldType = GetFieldTypeOrDefault(card);

            RequestOnBeginDrag?.Invoke(card, _dragFieldType);
            CacheDragPointerOffset(eventData);
        }

        public void HandleOnDrag(Card card, PointerEventData eventData)
        {
            _lastPointerPosition = eventData.position;

            if (!_isDragging)
            {
                return;
            }

            if (_isDragCanceled)
            {
                return;
            }

            if (_fieldSystem.HoverCard == null)
            {
                CompleteDrag(false);
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                CancelDrag(card);
                return;
            }
            
            MoveHoverCardToMousePosition(eventData);

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

            if (!_isDragging)
            {
                if (_fieldSystem.HoverCard != null)
                {
                    _dragFieldType = GetFieldTypeOrDefault(card);
                    CompleteDrag(false);
                }

                return;
            }

            if (_isDragCanceled)
            {
                CompleteDrag(false, false);
                return;
            }

            if (_fieldSystem.HoverCard == null)
            {
                CompleteDrag(false);
                return;
            }

            if (!CardDragRaycastResolver.IsInsideScreen(eventData.position))
            {
                CancelDrag(card);
                return;
            }
            
            var success = false;
            try
            {
                success = TryInvokeDropHandler(eventData.position);
            }
            finally
            {
                CompleteDrag(success);
            }
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            // Drop commits are executed by the drag source after cleanup.
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

        private FieldType GetFieldTypeOrDefault(Card card)
        {
            if (card == null)
            {
                return _fieldSystem.FieldType;
            }

            return card.CardType switch
            {
                CardType.AtkField => FieldType.Attack,
                CardType.DefField => FieldType.Defense,
                _ => _fieldSystem.FieldType
            };
        }

        private void MoveHoverCardToMousePosition(PointerEventData eventData)
        {
            var hoverCard = _fieldSystem.HoverCard;
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
            var hoverCard = _fieldSystem.HoverCard;
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

        private void CancelDrag(Card card)
        {
            if (_isDragCanceled)
            {
                return;
            }

            _isDragCanceled = true;
            _fieldSystem.HoverCard?.Pointer.CancelDrag();
            CompleteDrag(false);
        }

        private void CompleteDrag(bool success, bool requestEndDrag = true)
        {
            if (requestEndDrag && !_isEndDragRequested)
            {
                _isEndDragRequested = true;
                RequestOnEndDrag?.Invoke(success, _dragFieldType);
            }

            var pendingDropCommit = success ? _pendingDropCommit : null;
            _pendingDropCommit = null;
            _isDragging = false;
            _isDragCanceled = false;
            _dragPointerOffset = Vector2.zero;
            pendingDropCommit?.Invoke();
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
            var handHit = CheckField(position, out _);
            if (handHit == null)
            {
                return false;
            }
            
            var handEventSystem = CardDragRaycastResolver.GetComponentInParent<HandEventSystem>(handHit.Value);
            if (handEventSystem == null)
            {
                return false;
            }

            var hoverCard = _fieldSystem.HoverCard;
            if (hoverCard == null)
            {
                return false;
            }

            _pendingDropCommit = () => _dropSystem.OnDroppedFieldToHand(hoverCard);
            return true;
        }
    }
}
