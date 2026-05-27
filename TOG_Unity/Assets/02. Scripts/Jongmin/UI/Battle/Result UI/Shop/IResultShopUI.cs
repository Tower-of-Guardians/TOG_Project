public interface IResultShopUI : IOpenableUI
{
    void Construct(ResultShopPresenter resultShopPresenter);
    void Initialize();
    void UpdateRate(string rateString);
    void UpdateRefresh(string refreshString, bool canRefresh);
}