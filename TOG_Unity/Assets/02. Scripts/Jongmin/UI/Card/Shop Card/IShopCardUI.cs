public interface IShopCardUI : ICardUI
{
    void Construct(ShopCardPresenter shopCardPresenter);
    void InitUI(BattleCardData battleCardData, bool canPurchase);
    void UpdateUI(BattleCardData battleCardData, bool canPurchase);
}