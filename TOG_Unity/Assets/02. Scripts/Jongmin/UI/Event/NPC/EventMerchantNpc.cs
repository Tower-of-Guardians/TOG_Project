using System.Collections;
using UnityEngine.EventSystems;
using VContainer;

public class EventMerchantNpc : EventNpc
{
    private MerchantPresenter _merchantPresenter;
    
    [Inject]
    private void Construct(MerchantPresenter merchantPresenter)
    {
        _merchantPresenter = merchantPresenter;
    }
    
    public override IEnumerator HandleOnEventBegin()
    {
        yield return null;
    }

    public override IEnumerator HandleOnEventEnd()
    {
        yield return null;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        _merchantPresenter.OpenUI();
    }
}