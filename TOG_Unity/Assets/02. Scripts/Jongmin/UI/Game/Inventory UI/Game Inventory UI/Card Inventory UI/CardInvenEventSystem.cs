using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class CardInvenEventSystem : MonoBehaviour
    {
        public event Action<CardData> RequestShowCardInfo;
        
        public void Subscribe(Card card)
        {
            card.Pointer.OnPointerClicked += HandleOnPointerClick;
        }

        public void Unsubscribe(Card card)
        {
            card.Pointer.OnPointerClicked -= HandleOnPointerClick;
        }

        public void HandleOnPointerClick(Card card, PointerEventData eventData)
        {
            RequestShowCardInfo?.Invoke(card.CardData);
        }
    }
}