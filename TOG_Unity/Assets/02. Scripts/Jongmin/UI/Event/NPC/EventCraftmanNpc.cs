using System.Collections;
using UnityEngine.EventSystems;
using VContainer;

public class EventCraftmanNpc : EventNpc
{
    private CraftmanPresenter _craftmanPresenter;
    
    [Inject]
    private void Construct(CraftmanPresenter craftmanPresenter)
    {
        _craftmanPresenter = craftmanPresenter;
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
        _craftmanPresenter.OpenUI();
    }
}