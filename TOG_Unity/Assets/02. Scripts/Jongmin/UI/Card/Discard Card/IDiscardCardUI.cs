using System;
using UnityEngine.EventSystems;
using UnityEngine;

public interface IDiscardCardUI : ICardUI,
                                  IBeginDragHandler,
                                  IDragHandler,
                                  IEndDragHandler
{
    public event Action OnBeginDragAction;
    public event Action<Vector2> OnDragAction;
    public event Action OnEndDragAction;
}