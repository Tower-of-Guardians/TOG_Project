using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class ToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image buttonImage;

    [Header("On References")]
    [SerializeField] private Sprite onNormalSprite;
    [SerializeField] private Sprite onHoverSprite;
    [SerializeField] private Sprite onClickedSprite;
    
    [Header("Off References")]
    [SerializeField] private Sprite offNormalSprite;
    [SerializeField] private Sprite offHoverSprite;
    [SerializeField] private Sprite offClickedSprite;

    private bool _isSelected = true;
    public event Action OnClick;

    private void Awake()
    {
        buttonImage.sprite = onNormalSprite;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.sprite = _isSelected ? onHoverSprite 
                                         : offHoverSprite;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.sprite = _isSelected ? onClickedSprite 
                                         : offClickedSprite;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        _isSelected = !_isSelected;
        OnClick?.Invoke();
        
        buttonImage.sprite = _isSelected ? onHoverSprite 
                                         : offHoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.sprite = _isSelected ? onNormalSprite 
                                         : offNormalSprite;
    }

    public void Reset()
    {
        _isSelected = true;
        buttonImage.sprite = onHoverSprite;
    }
}