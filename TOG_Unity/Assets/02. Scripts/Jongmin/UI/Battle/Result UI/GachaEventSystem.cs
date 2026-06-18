using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class GachaEventSystem : MonoBehaviour
    {
        public event Action<CardData> RequestShowCardInfo;

        public void Subscribe(Card card)
        {
            card.Pointer.OnPointerClicked += HandleOnPointerClicked;
        }

        public void Unsubscribe(Card card)
        {
            card.Pointer.OnPointerClicked -= HandleOnPointerClicked;
        }

        private void HandleOnPointerClicked(Card card, PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                return;
            }
            
            RequestShowCardInfo?.Invoke(card.CardData);
        }
    }
}