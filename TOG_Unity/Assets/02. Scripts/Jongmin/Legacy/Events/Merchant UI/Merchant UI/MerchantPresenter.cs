using System;

public class MerchantPresenter
{
    private readonly IMerchantUI _merchantUI;
    private readonly ShopPresenter _shopPresenter;
    private bool _isOpen;

    public event Action Closed;

    public MerchantPresenter(IMerchantUI merchantUI,
                             ShopPresenter shopPresenter)
    {
        _merchantUI = merchantUI;
        _shopPresenter = shopPresenter;
        
        _merchantUI.Construct(this);
    }
    public void OpenUI()
        => TryOpenUI();

    public bool TryOpenUI()
    {
        if (_isOpen)
        {
            return true;
        }

        if (!_shopPresenter.TryOpenUI())
        {
            return false;
        }

        _isOpen = true;
        _merchantUI.OpenUI();
        return true;
    }

    public void CloseUI()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        _shopPresenter.CloseUI();
        _merchantUI.CloseUI();
        Closed?.Invoke();
    }
}
