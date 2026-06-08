using UnityEngine;

public class TooltipUIInjector : MonoBehaviour, IInjector
{
    [Header("의존성 목록")]
    [Header("툴팁 뷰")]
    [SerializeField] private TooltipView m_tooltip_view;

    [Header("캔버스")]
    [SerializeField] private Canvas m_canvas;

    [Header("플레이어")]
    [SerializeField] private PlayerDescriptor m_player_descriptor;

    [Header("몬스터")]
    [SerializeField] private MonsterDescriptor[] m_monster_descriptors;

    [Header("턴 규칙 관리자")]
    [SerializeField] private TurnManager m_turn_manager;

    [Header("행동 메뉴얼 툴팁")]
    [SerializeField] private ActionManualDescriptableUI m_action_manual_tooltip;

    [Header("교체 버튼 툴팁")]
    [SerializeField] private ThrowButtonDescriptableUI m_throw_button_tooltip;

    [Header("교체 메뉴얼 툴팁")]
    [SerializeField] private ThrowManualDescriptableUI m_throw_manual_tooltip;

    [Header("후보 카드 덱 툴팁")]
    [SerializeField] private DrawButtonDescriptableUI m_draw_button_tooltip;

    private TooltipPresenter m_tooltip_presenter;

    private IDescriptableUI[] m_descriptables;

    private void Awake()
    {
        m_descriptables = m_canvas.GetComponentsInChildren<IDescriptableUI>();
    }

    public void Inject()
    {
        InstallTooltip();
        InjectTooltip();
        InjectBattleTooltip();
    }

    private void InstallTooltip()
    {
        DIContainer.Register<ITooltipView>(m_tooltip_view);

        m_tooltip_presenter = new TooltipPresenter(m_tooltip_view);
        DIContainer.Register<TooltipPresenter>(m_tooltip_presenter);
    }

    private void InjectBattleTooltip()
    {
        m_action_manual_tooltip.Inject(m_turn_manager,
                                       m_tooltip_presenter);

        m_throw_button_tooltip.Inject(m_turn_manager,
                                      m_tooltip_presenter);

        m_throw_manual_tooltip.Inject(m_turn_manager,
                                      m_tooltip_presenter);

        m_draw_button_tooltip.Inject(m_turn_manager,
                                     m_tooltip_presenter);
    }

    private void InjectTooltip()
    {
        var tooltip_presenter = DIContainer.Resolve<TooltipPresenter>();

        foreach (var descriptable in m_descriptables)
        {
            if (descriptable == null)
            {
                continue;
            }

            descriptable.Inject(tooltip_presenter);
        }

        TempInject(tooltip_presenter);
    }

    private void TempInject(TooltipPresenter tooltip_presenter)
    {
        if (m_player_descriptor != null)
        {
            m_player_descriptor.Inject(tooltip_presenter);
        }

        if (m_monster_descriptors == null)
        {
            return;
        }

        foreach (var descriptor in m_monster_descriptors)
        {
            if (descriptor == null)
            {
                Debug.LogWarning("TooltipUIInjector: Monster Descriptors에 비어 있는 항목이 있습니다. 씬에 배치한 몬스터의 MonsterDescriptor를 연결해 주세요.", this);
                continue;
            }

            descriptor.Inject(tooltip_presenter);
        }
    }
}