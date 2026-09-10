using JxModule;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jongmin
{
    public class CompactInvenSlot : ViewBase
    {
        [SerializeField] private Card card;
        [SerializeField] private LabelBoxView selectedImage;

        private CompactInvenSlotBehaviourBase _slotBehaviour;
        
        public Card Card => card;
        
        private void OnEnable()
        {
            _slotBehaviour = null;
            
            card.View.CanvasGroup.interactable = false;
            card.View.CanvasGroup.blocksRaycasts = false;
            
            CanvasGroup.Show();
            selectedImage.CanvasGroup.Hide();
            selectedImage.CanvasGroup.blocksRaycasts = false;
        }

        public void SetSlotBehaviour(CompactInvenSlotBehaviourBase slotBehaviour)
        {
            _slotBehaviour = slotBehaviour;
        }

        public void SetSelected(bool isSelected)
        {
            selectedImage.CanvasGroup.alpha = isSelected ? 1f : 0f;
            selectedImage.CanvasGroup.SetInteractable(false);
            selectedImage.CanvasGroup.blocksRaycasts = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _slotBehaviour?.OnPointerDown(this);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            _slotBehaviour?.OnPointerUp(this);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            _slotBehaviour?.OnPointerClick(this);
        }
    }
}
