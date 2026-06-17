using JxModule;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class TabButtonView : ButtonView
    {
        [SerializeField] private TabType tabType;
        
        [Header("Sprites")]
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite hoverSprite;
        [SerializeField] private Sprite selectedSprite;

        [Header("Tween Settings")]
        [SerializeField] private float normalX = 2f;
        [SerializeField] private float selectedX = 14f;
        [SerializeField] private Vector2 normalSize = new(72f, 252f);
        [SerializeField] private Vector2 selectedSize = new(128f, 352f);
        
        private bool _isSelected;
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isSelected)
            {
                Image.sprite = hoverSprite;
            }
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!_isSelected)
            {
                Image.sprite = normalSprite;
            }
        }
    
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (_isSelected)
            {
                return;
            }

            SelectButton();
        }
        
        public void SelectButton()
        {
            _isSelected = true;
            Image.sprite = selectedSprite;
            RectTransform.sizeDelta = selectedSize;
            RectTransform.anchoredPosition = new Vector2(selectedX, RectTransform.anchoredPosition.y);
            RectTransform.SetAsLastSibling();
        }

        public void DeselectButton(TabType currentTabType)
        {
            if (currentTabType == tabType)
            {
                return;
            }
        
            _isSelected = false;
            Image.sprite = normalSprite;
            RectTransform.sizeDelta = normalSize;
            RectTransform.anchoredPosition = new Vector2(normalX, RectTransform.anchoredPosition.y);
        }
    }
}