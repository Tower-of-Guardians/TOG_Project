namespace Jongmin
{
    public interface ICardDropTarget<T>
    {
        void CreateCard(BattleCardData battleCardData);
        void RemoveCard(Card card, bool isUpdateLayout = true);
    }
}
