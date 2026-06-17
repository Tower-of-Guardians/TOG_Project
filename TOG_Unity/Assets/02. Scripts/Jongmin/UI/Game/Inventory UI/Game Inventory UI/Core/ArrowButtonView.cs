using JxModule;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jongmin
{
    public class ArrowButtonView : ButtonView
    {
        [Header("Sort Box")]
        [SerializeField] private Image sortBoxImage;
        [SerializeField] private Sprite normalBoxSprite;
        [SerializeField] private Sprite hoverBoxSprite;
        [SerializeField] private Sprite clickedBoxSprite;
        
        [Header("Button")]
        [SerializeField] private Sprite normalBtnSprite;
        [SerializeField] private Sprite hoverBtnSprite;
        [SerializeField] private Sprite clickedBtnSprite;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            sortBoxImage.sprite = hoverBoxSprite;
            Image.sprite = hoverBtnSprite;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            sortBoxImage.sprite = normalBoxSprite;
            Image.sprite = normalBtnSprite;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            sortBoxImage.sprite = clickedBoxSprite;
            Image.sprite = clickedBtnSprite;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            sortBoxImage.sprite = hoverBoxSprite;
            Image.sprite = hoverBtnSprite;
        }
    }
}