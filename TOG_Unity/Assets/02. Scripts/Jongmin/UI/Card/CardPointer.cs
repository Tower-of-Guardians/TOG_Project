using System;
using JxModule;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    [RequireComponent(typeof(JxEmptyGraphic))]
    public class CardPointer : MonoBehaviour, 
                               IPointerEnterHandler, 
                               IPointerExitHandler, 
                               IPointerClickHandler, 
                               IPointerDownHandler, 
                               IPointerUpHandler, 
                               IBeginDragHandler, 
                               IDragHandler, 
                               IEndDragHandler
    {
        [SerializeField] private CanvasGroup cardGroup;
        
        private Card _owner;
        private bool _isDragging;
        
        public event Action<Card, PointerEventData> OnPointerEntered;
        public event Action<Card, PointerEventData> OnPointerExited;
        public event Action<Card, PointerEventData> OnPointerClicked;
        public event Action<Card, PointerEventData> OnPointerDowned;
        public event Action<Card, PointerEventData> OnPointerUpped;
        public event Action<Card, PointerEventData> OnBeginDragged;
        public event Action<Card, PointerEventData> OnDragged;
        public event Action<Card, PointerEventData> OnEndDragged;

        public bool IsDragCanceled { get; private set; }

        public void SetOwner(Card owner)
        {
            _owner = owner;
        }

        public void SetInteraction(bool isActive)
        {
            if (cardGroup == null)
            {
                return;
            }

            cardGroup.interactable = isActive;
            cardGroup.blocksRaycasts = isActive;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEntered?.Invoke(_owner, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExited?.Invoke(_owner, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClicked?.Invoke(_owner, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDowned?.Invoke(_owner, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpped?.Invoke(_owner, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanReceivePointerEvents())
            {
                _isDragging = false;
                return;
            }

            _isDragging = true;
            IsDragCanceled = false;
            OnBeginDragged?.Invoke(_owner, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            OnDragged?.Invoke(_owner, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            var previousBlocksRaycasts = cardGroup == null || cardGroup.blocksRaycasts;

            try
            {
                if (cardGroup != null)
                {
                    cardGroup.blocksRaycasts = false;
                }

                OnEndDragged?.Invoke(_owner, eventData);
            }
            finally
            {
                _isDragging = false;

                if (cardGroup != null)
                {
                    cardGroup.blocksRaycasts = previousBlocksRaycasts;
                }
            }
        }

        public void CancelDrag()
        {
            IsDragCanceled = true;
        }

        private bool CanReceivePointerEvents()
        {
            return cardGroup == null || cardGroup.interactable && cardGroup.blocksRaycasts;
        }
    }
}
