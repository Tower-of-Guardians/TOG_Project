using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite selectedSprite;
    
    [Header("Offset References")]
    [SerializeField] private float normalX = 2f;
    [SerializeField] private float selectedX = 14f;
    [SerializeField] private Vector2 normalSize = new(72f, 252f);
    [SerializeField] private Vector2 selectedSize = new(128f, 352f);

    [Header("Type References")]
    [SerializeField] private TabType tabType;
    
    private bool _isSelected;
    public event Action OnClick;

    public void Construct(InventoryTabPresenter inventoryTabPresenter)
    {
        inventoryTabPresenter.OnDeselectTab += DeselectButton;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isSelected)
        {
            buttonImage.sprite = hoverSprite;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected)
        {
            return;
        }

        SelectButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isSelected)
        {
            buttonImage.sprite = normalSprite;
        }
    }

    public void SelectButton()
    {
        _isSelected = true;
        buttonImage.sprite = selectedSprite;
        buttonImage.rectTransform.sizeDelta = selectedSize;
        buttonImage.rectTransform.anchoredPosition = new Vector2(selectedX, buttonImage.rectTransform.anchoredPosition.y);
        buttonImage.rectTransform.SetAsLastSibling();

        OnClick?.Invoke();
    }

    private void DeselectButton(TabType currentTabType)
    {
        if (currentTabType == tabType)
        {
            return;
        }
        
        _isSelected = false;
        buttonImage.sprite = normalSprite;
        buttonImage.rectTransform.sizeDelta = normalSize;
        buttonImage.rectTransform.anchoredPosition = new Vector2(normalX, buttonImage.rectTransform.anchoredPosition.y);
    }
}