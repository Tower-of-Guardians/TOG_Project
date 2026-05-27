
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IFieldCardUI : ICardUI,
                                IBeginDragHandler,
                                IDragHandler,
                                IEndDragHandler
{
    event Action OnBeginDragAction;
    event Action<Vector2> OnDragAction;
    event Action OnEndDragAction;

    void UpdateUI(CardData cardData, bool isAtk);
    void ToggleLock();
}