public class MerchantPresenter
{
    private readonly IMerchantUI _merchantUI;
    private readonly ShopPresenter _shopPresenter;

    public MerchantPresenter(IMerchantUI merchantUI,
                             ShopPresenter shopPresenter)
    {
        _merchantUI = merchantUI;
        _shopPresenter = shopPresenter;
        
        _merchantUI.Construct(this);
    }
    public void OpenUI()
    {
        // TODO: 대화 이벤트 등록
        OpenShop();
        _merchantUI.OpenUI();
    }

    public void CloseUI()
    {
        // TODO: 대화 이벤트 해제
        CloseShop();
        _merchantUI.CloseUI();
    }
    
    private void OpenShop()
        => _shopPresenter.OpenUI();

    private void CloseShop()
        => _shopPresenter.CloseUI();
}
