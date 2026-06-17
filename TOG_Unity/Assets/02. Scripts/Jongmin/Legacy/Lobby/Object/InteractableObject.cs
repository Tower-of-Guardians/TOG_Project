using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnMouseEnterAction;
    public event Action OnMouseExitAction;
    public event Action OnMouseDownAction;
    public event Action OnMouseUpAction;



    public void OnPointerEnter(PointerEventData eventData)
        => OnMouseEnterAction?.Invoke();

    public void OnPointerExit(PointerEventData eventData)
        => OnMouseExitAction?.Invoke();

    public void OnPointerDown(PointerEventData eventData)
        => OnMouseDownAction?.Invoke();

    public void OnPointerUp(PointerEventData eventData)
        => OnMouseUpAction?.Invoke();
}
