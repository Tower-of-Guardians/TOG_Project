public interface IResultCardUI : ICardUI
{
    void Construct(ResultCardPresenter resultCardPresenter);
    void UpdatePurchase(bool isSoldOut);
    void UpdateUI(BattleCardData slot_data, bool canPurchase);
}