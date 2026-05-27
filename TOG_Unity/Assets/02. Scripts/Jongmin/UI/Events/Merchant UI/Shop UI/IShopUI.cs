public interface IShopUI : IOpenableUI
{
    void Construct(ShopPresenter shopPresenter);
    void ToggleSaleButton(bool active);
}