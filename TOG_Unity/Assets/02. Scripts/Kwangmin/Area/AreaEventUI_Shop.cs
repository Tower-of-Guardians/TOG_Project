using Jongmin;
using UnityEngine;

public class AreaEventUI_Shop : AreaEventSubUI
{
    private MerchantDomain _merchantDomain;
    private bool _hasOpened;

    public override void Open()
    {
        if (IsOpen)
        {
            return;
        }

        if (!DIContainer.IsRegistered<MerchantDomain>())
        {
            Debug.LogError("MerchantDomain이 DIContainer에 등록되어 있지 않습니다.", this);
            return;
        }

        _merchantDomain = DIContainer.Resolve<MerchantDomain>();
        if (_merchantDomain == null) return;

        _merchantDomain.OpenView(!_hasOpened);
        if (!_merchantDomain.IsOpen)
        {
            _merchantDomain = null;
            return;
        }

        _merchantDomain.ViewClosed += HandleMerchantClosed;
        _hasOpened = true;
        base.Open(false);
    }

    public override void Close()
    {
        if (_merchantDomain != null && _merchantDomain.IsOpen)
        {
            _merchantDomain.CloseView();
            return;
        }

        HandleMerchantClosed();
    }

    private void HandleMerchantClosed()
    {
        ReleaseMerchant();
        base.Close();
    }

    private void OnDisable()
    {
        MerchantDomain domain = _merchantDomain;
        ReleaseMerchant();
        if (domain != null && domain.IsOpen)
        {
            domain.CloseView();
        }
    }

    private void ReleaseMerchant()
    {
        if (_merchantDomain == null)
        {
            return;
        }

        _merchantDomain.ViewClosed -= HandleMerchantClosed;
        _merchantDomain = null;
    }
}
