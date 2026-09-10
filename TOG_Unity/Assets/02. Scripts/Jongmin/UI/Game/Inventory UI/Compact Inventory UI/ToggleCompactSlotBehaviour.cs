namespace Jongmin
{
    public class ToggleCompactSlotBehaviour : CompactInvenSlotBehaviourBase
    {
        private readonly IMultiSelectableCompactInvenSystem _selectableSystem;

        public ToggleCompactSlotBehaviour(IMultiSelectableCompactInvenSystem selectableSystem)
        {
            _selectableSystem = selectableSystem;
        }

        public override void OnPointerClick(CompactInvenSlot invenSlot)
        {
            _selectableSystem.ToggleSlot(invenSlot);
        }
    }
}
