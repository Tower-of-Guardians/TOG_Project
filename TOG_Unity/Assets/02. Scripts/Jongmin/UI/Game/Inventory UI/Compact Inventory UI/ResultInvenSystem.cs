namespace Jongmin
{
    public class ResultInvenSystem : CompactInvenSystem
    {
        protected override void CreateSlot(CardData cardData)
        {
            var slot = Factory.Create(CompactInvenType.Result);
            slot.SetSlotBehaviour(new ReadOnlyCompactSlotBehaviour());
            slot.Card.SetCardData(cardData);
            Container.Add(slot);
        }
    }
}