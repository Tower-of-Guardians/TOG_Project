using System.Linq;

namespace Jongmin
{
    public sealed class PlayerCardInventoryProgress : ICardInventory
    {
        public int CardCount => DataCenter.Instance.userDeck != null ? DataCenter.Instance.userDeck.Count : 0;

        public bool HasCard(string cardID)
        {
            if (string.IsNullOrWhiteSpace(cardID) || DataCenter.Instance.userDeck == null)
            {
                return false;
            }

            return DataCenter.Instance.userDeck.Any(card => card != null && card.id == cardID);
        }
    }
}
