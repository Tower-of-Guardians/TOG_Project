using UnityEngine;
using UnityEngine.EventSystems;

public class DrawButtonDescriptableUI : MonoBehaviour, IDescriptableUI
{
    [Header("툴팁의 위치")]
    [SerializeField] private Vector3 m_tooltip_position;

    private TooltipPresenter m_tooltip_presenter;
    private TurnManager m_turn_manager;

    public void Inject(TurnManager turn_manager,
                       TooltipPresenter tooltip_presenter)
    {
        m_turn_manager = turn_manager;
        Inject(tooltip_presenter);
    }

    public void Inject(TooltipPresenter tooltip_presenter)
        => m_tooltip_presenter = tooltip_presenter;

    public TooltipData GetTooltipData()
        => new()
           {
                Description = "<color=#99CCFF>후보 카드 덱</color>\n\n"
                            + $"턴마다 <color=#99CCFF>{m_turn_manager.MaxHandCount}</color>장의 카드를 뽑습니다.",
                Position = m_tooltip_position
           };

    public void OnPointerEnter(PointerEventData eventData)
        => m_tooltip_presenter.OpenUI(this);

    public void OnPointerExit(PointerEventData eventData)
        => m_tooltip_presenter.CloseUI();
}
