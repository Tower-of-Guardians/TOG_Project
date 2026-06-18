using JxModule;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class ToggleButtonView : ButtonView
    {
        [Header("On")]
        [SerializeField] private Sprite onNormalSprite;
        [SerializeField] private Sprite onHoverSprite;
        [SerializeField] private Sprite onClickedSprite;
        
        [Header("Off")]
        [SerializeField] private Sprite offNormalSprite;
        [SerializeField] private Sprite offHoverSprite;
        [SerializeField] private Sprite offClickedSprite;

        private bool _isSelected = true;
        
        private void Awake()
        {
            Image.sprite = onNormalSprite;
        }
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            Image.sprite = _isSelected ? onHoverSprite : offHoverSprite;
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            Image.sprite = _isSelected ? onNormalSprite : offNormalSprite;
        }
    
        public override void OnPointerDown(PointerEventData eventData)
        {
            Image.sprite = _isSelected ? onClickedSprite : offClickedSprite;
        }
    
        public override void OnPointerUp(PointerEventData eventData)
        {
            _isSelected = !_isSelected;
            Image.sprite = _isSelected ? onHoverSprite : offHoverSprite;
        }
        
        public void Reset()
        {
            _isSelected = true;
            Image.sprite = onHoverSprite;
        }
    }
}