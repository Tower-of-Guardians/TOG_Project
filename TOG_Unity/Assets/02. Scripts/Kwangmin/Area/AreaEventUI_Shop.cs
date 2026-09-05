using UnityEngine;
using VContainer;

public class AreaEventUI_Shop : AreaEventSubUI
{
    private MerchantPresenter _merchantPresenter;

    public override void Open()
    {
        if (IsOpen)
        {
            return;
        }

        GameLifetimeScope lifetimeScope = FindAnyObjectByType<GameLifetimeScope>(FindObjectsInactive.Include);
        if (lifetimeScope == null || lifetimeScope.Container == null ||
            !lifetimeScope.Container.TryResolve(out MerchantPresenter merchantPresenter))
        {
            Debug.LogError("상인 UI가 GameLifetimeScope에 준비되어 있지 않습니다.", this);
            return;
        }

        if (!merchantPresenter.TryOpenUI())
        {
            return;
        }

        _merchantPresenter = merchantPresenter;
        _merchantPresenter.Closed += Close;
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
        if (_merchantPresenter == null)
        {
            return;
        }

        MerchantPresenter merchantPresenter = _merchantPresenter;
        _merchantPresenter = null;
        merchantPresenter.Closed -= Close;
        merchantPresenter.CloseUI();
    }
}
