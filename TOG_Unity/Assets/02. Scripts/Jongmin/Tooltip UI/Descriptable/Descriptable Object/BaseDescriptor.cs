using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseDescriptor : MonoBehaviour, IDescriptable
{
    [Header("툴팁의 위치")]
    [SerializeField] protected Vector3 m_tooltip_position;

    protected TooltipPresenter m_tooltip_presenter;

    public void Inject(TooltipPresenter tooltip_presenter)
    {
        m_tooltip_presenter = tooltip_presenter;
    }

    public abstract TooltipData GetTooltipData();

    protected virtual void OnMouseEnter()
    {
        if(EventSystem.current.IsPointerOverGameObject())
            return;

        m_tooltip_presenter.OpenUI(this);
    }

    protected virtual void OnMouseOver()
    {
        if(EventSystem.current.IsPointerOverGameObject())
            return;
            
        m_tooltip_presenter.UpdateUI(this);
    }

    protected virtual void OnMouseExit()
    {
        if(EventSystem.current.IsPointerOverGameObject())
            return;
            
        m_tooltip_presenter.CloseUI();
    }
}
