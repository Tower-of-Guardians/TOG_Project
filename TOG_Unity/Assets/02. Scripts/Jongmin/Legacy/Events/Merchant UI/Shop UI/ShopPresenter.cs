using UnityEngine;

public class ShopPresenter
{
    private readonly IShopUI _shopUI;
    private readonly ShopDispenser _shopDispenser;
    private readonly MerchantDeckInvenPresenter _merchantDeckInvenPresenter;

    public ShopPresenter(IShopUI shopUI,
                         ShopDispenser shopDispenser,
                         MerchantDeckInvenPresenter merchantDeckInvenPresenter)
    {
        _shopUI = shopUI;
        _shopDispenser = shopDispenser;
        _merchantDeckInvenPresenter = merchantDeckInvenPresenter;

        _shopUI.Construct(this);
    }

    public void OpenUI()
        => TryOpenUI();

    public bool TryOpenUI()
    {
        if (!_shopDispenser.Initialize())
        {
            return false;
        }

        _merchantDeckInvenPresenter.CloseUI();
        _shopUI.ToggleSaleButton(true);
        _shopUI.OpenUI();
        return true;
    }

    public void CloseUI()
    {
        _merchantDeckInvenPresenter.CloseUI();
        _shopUI.CloseUI();
    }

    public void FadeUpUI()
        => _shopUI.OpenUI();

    public void FadeDownUI()
        => _shopUI.CloseUI();

    public void ToggleSaleButton(bool active)
        => _shopUI.ToggleSaleButton(active);

    public void OnClickedSale()
    {
        Debug.Log("OnClickedSale");
        FadeDownUI();
        _merchantDeckInvenPresenter.OpenUI();
    }
}
