using System;
using System.Collections.Generic;

namespace Jongmin
{
    public class MerchantInvenSystem : CompactInvenSystem, IMultiSelectableCompactInvenSystem
    {
        private readonly List<CardData> _selectedCards = new();
        private readonly HashSet<CompactInvenSlot> _selectedSlots = new();

        public event Action<IReadOnlyList<CardData>> OnSelectionChanged;
        public IReadOnlyList<CardData> SelectedCards => _selectedCards;

        protected override void CreateSlot(CardData cardData)
        {
            var slot = Factory.Create(CompactInvenType.Merchant);
            slot.SetSlotBehaviour(new ToggleCompactSlotBehaviour(this));
            slot.Card.SetCardData(cardData);
            slot.SetSelected(false);
            Container.Add(slot);
        }

        public void ToggleSlot(CompactInvenSlot invenSlot)
        {
            if (invenSlot == null || invenSlot.Card.CardData == null)
            {
                return;
            }

            if (_selectedSlots.Contains(invenSlot))
            {
                DeselectSlot(invenSlot);
            }
            else
            {
                SelectSlot(invenSlot);
            }

            OnSelectionChanged?.Invoke(SelectedCards);
        }

        public void ClearSelection()
        {
            foreach (var slot in _selectedSlots)
            {
                slot.SetSelected(false);
            }

            _selectedSlots.Clear();
            _selectedCards.Clear();
            OnSelectionChanged?.Invoke(SelectedCards);
        }

        public override void CloseView()
        {
            ClearSelection();
            base.CloseView();
        }

        private void SelectSlot(CompactInvenSlot invenSlot)
        {
            _selectedSlots.Add(invenSlot);
            _selectedCards.Add(invenSlot.Card.CardData);
            invenSlot.SetSelected(true);
        }

        private void DeselectSlot(CompactInvenSlot invenSlot)
        {
            _selectedSlots.Remove(invenSlot);
            _selectedCards.Remove(invenSlot.Card.CardData);
            invenSlot.SetSelected(false);
        }
    }
}
