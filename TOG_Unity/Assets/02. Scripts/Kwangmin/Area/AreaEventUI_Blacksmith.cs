using Jongmin;
using UnityEngine;

public class AreaEventUI_Blacksmith : AreaEventSubUI
{
    private CraftmanDomain _craftmanDomain;

    public override void Open()
    {
        if (IsOpen)
        {
            return;
        }

        if (!DIContainer.IsRegistered<CraftmanDomain>())
        {
            Debug.LogError("CraftmanDomain이 DIContainer에 등록되어 있지 않습니다.", this);
            return;
        }

        _craftmanDomain = DIContainer.Resolve<CraftmanDomain>();
        if (_craftmanDomain == null)
        {
            Debug.LogError("대장간 UI를 열 수 없습니다. CraftmanDomain 연결을 확인하세요.", this);
            return;
        }

        _craftmanDomain.ViewClosed += HandleCraftmanClosed;
        base.Open(false);
        _craftmanDomain.OpenView();
    }

    public override void Close()
    {
        if (_craftmanDomain != null && _craftmanDomain.IsOpen)
        {
            _craftmanDomain.CloseView();
            return;
        }

        HandleCraftmanClosed();
    }

    private void HandleCraftmanClosed()
    {
        ReleaseCraftman();
        base.Close();
    }

    private void ReleaseCraftman()
    {
        if (_craftmanDomain != null)
        {
            _craftmanDomain.ViewClosed -= HandleCraftmanClosed;
            _craftmanDomain = null;
        }
    }

    private void OnDisable()
    {
        CraftmanDomain domain = _craftmanDomain;
        ReleaseCraftman();
        if (domain != null && domain.IsOpen)
        {
            domain.CloseView();
        }
    }
}
