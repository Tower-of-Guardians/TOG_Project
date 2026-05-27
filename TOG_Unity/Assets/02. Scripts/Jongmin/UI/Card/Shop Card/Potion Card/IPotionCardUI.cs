public interface IPotionCardUI
{
    void Inject(PotionCardPresenter potionCardPresenter);
    void InitUI(int cost, bool canPurchase);
    void UpdateUI(int cost, bool canPurchase);
}