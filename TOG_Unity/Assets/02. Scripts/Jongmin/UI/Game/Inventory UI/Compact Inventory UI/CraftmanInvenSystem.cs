using System;

namespace Jongmin
{
    public class CraftmanInvenSystem : CompactInvenSystem, ISelectableCompactInvenSystem
    {
        public event Action<CardData> OnSelectedSlot;
        
        protected override void CreateSlot(CardData cardData)
        {
            var slot = Factory.Create(CompactInvenType.Result);
            slot.SetSlotBehaviour(new SelectCompactSlotBehaviour(this));
            slot.Card.SetCardData(cardData);
            Container.Add(slot);
        }

        public void SelectSlot(CardData cardData)
        {
            OnSelectedSlot?.Invoke(cardData);
        }
    }
}