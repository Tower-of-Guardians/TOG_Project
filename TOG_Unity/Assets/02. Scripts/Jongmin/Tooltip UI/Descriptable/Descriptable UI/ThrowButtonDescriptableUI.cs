using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowButtonDescriptableUI : MonoBehaviour, IDescriptableUI
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
                Description = m_turn_manager.CanThrow ? $"매 턴마다 최대 <color=#99CCFF>{m_turn_manager.MaxActionCount}</color>장의 카드를 <color=#99CCFF>1</color>회 교체할 수 있습니다."
                                                      : $"매 턴마다 최대 <color=#99CCFF>{m_turn_manager.MaxActionCount}</color>장의 카드를 <color=#99CCFF>1</color>회 교체할 수 있습니다.\n\n<color=#99CCFF>이 턴에 이미 교체를 했습니다.</color>",
                Position = m_tooltip_position
           };

    public void OnPointerEnter(PointerEventData eventData)
        => m_tooltip_presenter.OpenUI(this);

    public void OnPointerExit(PointerEventData eventData)
        => m_tooltip_presenter.CloseUI();
}
