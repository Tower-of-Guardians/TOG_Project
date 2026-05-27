public interface ICardDropTarget<T> where T : ICardUI
{
    void CreateCard(BattleCardData battleCardData);
    void RemoveCard(T cardUI, bool isUpdateLayout = true);

    bool TryGetBattleCardData(T cardUI, out BattleCardData battleCardData);
}