using System;
using System.Collections.Generic;

namespace Jongmin
{
    public class MerchantInvenSystem : CompactInvenSystem, IMultiSelectableCompactInvenSystem
    {
        private const int MaxSelectableCount = 3;

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
                if (_selectedSlots.Count >= MaxSelectableCount)
                {
                    return;
                }

                SelectSlot(invenSlot);
            }

            OnSelectionChanged?.Invoke(SelectedCards);
        }

        public void ClearSelection()
        {
            ClearSelectionWithoutNotify();
            OnSelectionChanged?.Invoke(SelectedCards);
        }

        public override void CloseView()
        {
            ClearSelectionWithoutNotify();
            base.CloseView();
        }

        public override void RefreshView()
        {
            ClearSelectionWithoutNotify();
            base.RefreshView();
        }

        private void ClearSelectionWithoutNotify()
        {
            foreach (var slot in _selectedSlots)
            {
                slot.SetSelected(false);
            }

            _selectedSlots.Clear();
            _selectedCards.Clear();
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
