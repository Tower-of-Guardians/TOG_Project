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
        }

        public void SetSlotBehaviour(CompactInvenSlotBehaviourBase slotBehaviour)
        {
            _slotBehaviour = slotBehaviour;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("들어옴");
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