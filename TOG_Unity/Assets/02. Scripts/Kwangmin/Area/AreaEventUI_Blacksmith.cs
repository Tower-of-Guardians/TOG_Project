using Jongmin;
using UnityEngine;

public class AreaEventUI_Blacksmith : AreaEventSubUI
{
    public override void Open()
    {
        base.Open();

        if (!DIContainer.IsRegistered<CraftmanDomain>())
        {
            Debug.LogError("CraftmanDomain이 DIContainer에 등록되어 있지 않습니다.", this);
            return;
        }

        DIContainer.Resolve<CraftmanDomain>().OpenView();
    }
}
