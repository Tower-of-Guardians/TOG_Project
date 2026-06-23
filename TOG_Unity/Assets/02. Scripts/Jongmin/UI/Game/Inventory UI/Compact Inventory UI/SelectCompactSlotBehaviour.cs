using UnityEngine;

namespace Jongmin
{
    public class SelectCompactSlotBehaviour : CompactInvenSlotBehaviourBase
    {
        private readonly ISelectableCompactInvenSystem _selectableSystem;

        public SelectCompactSlotBehaviour(ISelectableCompactInvenSystem selectableSystem)
        {
            _selectableSystem = selectableSystem;
        }
        
        public override void OnPointerClick(CompactInvenSlot invenSlot)
        {
            _selectableSystem.SelectSlot(invenSlot.Card.CardData);
        }
    }
}