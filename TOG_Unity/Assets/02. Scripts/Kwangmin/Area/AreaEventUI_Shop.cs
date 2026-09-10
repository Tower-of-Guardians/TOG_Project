using Jongmin;

public class AreaEventUI_Shop : AreaEventSubUI
{
    private MerchantDomain _merchantDomain;

    public override void Open()
    {
        if (IsOpen)
        {
            return;
        }

        _merchantDomain = DIContainer.Resolve<MerchantDomain>();
        _merchantDomain?.OpenView();
        
        _merchantDomain.ViewClosed += Close;

        base.Open(false);
    }

    public override void Close()
    {
        ReleaseMerchant();
        base.Close();
    }

    private void OnDisable()
    {
        ReleaseMerchant();
    }

    private void ReleaseMerchant()
    {
        if (_merchantDomain == null)
        {
            return;
        }

        _merchantDomain.ViewClosed -= Close;
        _merchantDomain.CloseView();
    }
}
