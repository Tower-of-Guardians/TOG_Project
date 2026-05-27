using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IHandCardUI : ICardUI,
                               IPointerEnterHandler, 
                               IPointerExitHandler,
                               IBeginDragHandler,
                               IDragHandler,
                               IEndDragHandler,
                               IPointerClickHandler
{
    public event Action OnPointerEnterAction;
    public event Action OnPointerExitAction;
    public event Action OnBeginDragAction;
    public event Action<Vector2> OnDragAction;
    public event Action OnEndDragAction;
    public event Action OnPointerClickAction;
}