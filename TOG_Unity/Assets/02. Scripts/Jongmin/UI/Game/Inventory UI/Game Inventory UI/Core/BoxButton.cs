using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class BoxButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Box references")]
    [SerializeField] private Image boxImage;
    [SerializeField] private Sprite normalBoxSprite;
    [SerializeField] private Sprite hoverBoxSprite;
    [SerializeField] private Sprite clickedBoxSprite;
    
    [Header("Button References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite normalButtonSprite;
    [SerializeField] private Sprite hoverButtonSprite;
    [SerializeField] private Sprite clickedButtonSprite;

    public event Action OnClick; 
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        boxImage.sprite = hoverBoxSprite;
        buttonImage.sprite = hoverButtonSprite;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        boxImage.sprite = clickedBoxSprite;
        buttonImage.sprite = clickedButtonSprite;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        boxImage.sprite = hoverBoxSprite;
        buttonImage.sprite = hoverButtonSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        boxImage.sprite = normalBoxSprite;
        buttonImage.sprite = normalButtonSprite;
    }
}