using UnityEngine;
using UnityEngine.EventSystems;

public class StaticDescriptableUI : MonoBehaviour, IDescriptableUI
{
    [Header("툴팁 관련 컴포넌트")]
    [Header("툴팁 데이터")]
    [SerializeField] TooltipData m_tooltip_data;

    private TooltipPresenter m_tooltip_presenter;

    public TooltipData GetTooltipData() => m_tooltip_data;

    public void Inject(TooltipPresenter tooltip_presenter)
    {
        m_tooltip_presenter = tooltip_presenter;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_tooltip_presenter.OpenUI(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_tooltip_presenter.CloseUI();
    }
}
