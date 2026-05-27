using System;
using VContainer.Unity;

public class HandPresenter : ICardDropTarget<IHandCardUI>, IHandCardCreatePort, IHandCardRemovePort, IInitializable
{
    private readonly IHandUI _handUI;
    private readonly CardContainer<IHandCardUI, HandCardPresenter> _handCardContainer;
    private readonly HandCardLayoutController _handCardLayout;
    private readonly ICardFactory<IHandCardUI> _handCardFactory;

    public event Action<bool> OnTogglePreviews;

    public IHandCardUI HoverCard { get; set; }

    public HandPresenter(IHandUI handUI,
                         CardContainer<IHandCardUI, HandCardPresenter> handCardContainer,
                         ICardFactory<IHandCardUI> handCardFactory,
                         HandCardLayoutController handCardLayout
                         /*ThrowPresenter throw_presenter*/)
    {
        _handUI = handUI;
        _handCardContainer = handCardContainer;
        _handCardFactory = handCardFactory;
        _handCardLayout = handCardLayout;

        //throw_presenter.Inject(this);
    }

    public void Initialize()
    {
        _handCardLayout.Construct(_handCardContainer, this);
    }

    /// <summary>
    /// battleCardData에 해당하는 카드를 생성합니다.
    /// </summary>
    public void CreateCard(BattleCardData battleCardData)
    {
        IHandCardUI handCardUI = _handCardFactory.Create();
        HandCardPresenter handCardPresenter = new(handCardUI, battleCardData);

        if(!_handCardContainer.Add(handCardUI, handCardPresenter))
        {
            return;
        }

        _handCardLayout.UpdateLayout();
    }

    /// <summary>
    /// 해당 카드를 삭제하면서 레이아웃 재조정 여부를 통해 레이아웃을 재조정합니다.
    /// </summary>
    public void RemoveCard(IHandCardUI handCardUI, bool isUpdateLayout = true)
    {
        if(_handCardContainer.Remove(handCardUI))
        {
            _handCardFactory.Release(handCardUI);

            if(isUpdateLayout)
                _handCardLayout.UpdateLayout();
        }
    }

    public bool TryRemoveCard(BattleCardData battleCardData)
    {
        if(!_handCardContainer.TryGetUI(battleCardData, out IHandCardUI handCardUI))
        {
            return false;
        }

        RemoveCard(handCardUI);
        return true;
    }

    /// <summary>
    /// 카드 프리뷰 토글 이벤트를 발생시킵니다.
    /// </summary>
    public void ToggleFieldPreview(bool isActive)
        => OnTogglePreviews?.Invoke(isActive);
        //m_attack_field_presenter.ToggleManual(active);
        //m_defend_field_presenter.ToggleManual(active);
        //m_throw_presenter.ToggleManual(active);

    /// <summary>
    /// 카드를 통해 카드의 데이터와 탐색 성공 여부를 반환합니다.
    /// </summary>
    public bool TryGetBattleCardData(IHandCardUI cardUI, out BattleCardData battleCardData)
        => _handCardContainer.TryGetCardData(cardUI, out battleCardData);

    [Obsolete]
    public BattleCardData GetCardData(IHandCardUI cardUI)
    {
        _handCardContainer.TryGetCardData(cardUI, out BattleCardData battleCardData);
        return battleCardData;
    }
}
