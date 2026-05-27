using System;
using VContainer.Unity;

public class DiscardPresenter : IDisposable, IInitializable, ICardDropTarget<IDiscardCardUI>, IDiscardCardRemovePort
{
    private readonly IDiscardUI _discardUI;
    private readonly CardContainer<IDiscardCardUI, DiscardCardPresenter> _discardCardContainer;
    private readonly ICardFactory<IDiscardCardUI> _discardCardFactory;
    private readonly DiscardCardLayoutController _discardCardLayout;
    
    private readonly TurnManager _turnManager;

    private ThrowCardToHandEffector _discardCardToHandEffector;
    private ThrowCardToThrowEffector _discardCardToDiscardDeckEffector;

    public event Action<bool> OnDiscardUIVisibilityChanged;

    public IDiscardCardUI HoverCard { get; set; }

    public DiscardPresenter(IDiscardUI discardUI,
                            CardContainer<IDiscardCardUI, DiscardCardPresenter> discardCardContainer,
                            ICardFactory<IDiscardCardUI> discardCardFactory,
                            DiscardCardLayoutController discardCardLayout,
                            TurnManager turn_manager)
    {
        _discardUI = discardUI;
        _discardCardContainer = discardCardContainer;
        _discardCardFactory = discardCardFactory;
        _discardCardLayout = discardCardLayout;

        _turnManager = turn_manager;
    }

    public void BindEffectors(ThrowCardToHandEffector discardCardToHandEffector,
                              ThrowCardToThrowEffector discardCardToDiscardDeckEffector)
    {
        _discardCardToHandEffector = discardCardToHandEffector;
        _discardCardToDiscardDeckEffector = discardCardToDiscardDeckEffector;
    }

    public void Initialize()
    {
        _discardCardLayout.Construct(this, _discardCardContainer);

        _turnManager.OnUpdatedThrowActionState += UpdateOpenButtonState;
        _turnManager.OnUpdatedThrowCount += UpdateDiscardCount;
        _turnManager.Initialize();
    }

    /// <summary>
    /// battleCardData에 해당하는 카드를 생성합니다.
    /// </summary>
    public void CreateCard(BattleCardData battleCardData)
    {
        IDiscardCardUI discardCardUI = _discardCardFactory.Create();
        DiscardCardPresenter discardCardPresenter = new(discardCardUI, battleCardData);

        _discardCardContainer.Add(discardCardUI, discardCardPresenter);
        _discardCardLayout.UpdateLayout(false, false);
        _turnManager.UpdateThrowCount(1);
    }

    /// <summary>
    /// 해당 카드를 삭제하면서 레이아웃 재조정 여부를 통해 레이아웃을 재조정합니다.
    /// </summary>
    public void RemoveCard(IDiscardCardUI cardUI, bool unused = true)
    {
        if(!_discardCardContainer.Remove(cardUI))
        {
            return;
        }

        _discardCardFactory.Release(cardUI);
        _discardCardLayout.UpdateLayout(false, false);
        _turnManager.UpdateThrowCount(-1);
    }

    public bool TryRemoveCard(BattleCardData battleCardData)
    {
        if(!_discardCardContainer.TryGetUI(battleCardData, out IDiscardCardUI cardUI))
        {
            return false;
        }

        RemoveCard(cardUI);
        return true;
    }

    /// <summary>
    /// 카드를 통해 카드의 데이터와 탐색 성공 여부를 반환합니다.
    /// </summary>
    public bool TryGetBattleCardData(IDiscardCardUI cardUI, out BattleCardData battleCardData)
        => _discardCardContainer.TryGetCardData(cardUI, out battleCardData);

    /// <summary>
    /// 교체 가능성을 확인하고 가능하다면 프리뷰를 가시화합니다.
    /// </summary>
    public void TogglePreview(bool isActive)
    {
        if(_turnManager.CanThrow || !isActive)
        {
            _discardCardLayout.UpdateLayout(isActive, isActive, isActive);
            _discardUI.TogglePreview(isActive);
        }
    }

    public void OnClickedOpenButton()
    {
        _discardUI.OpenUI();
        OnDiscardUIVisibilityChanged?.Invoke(true);
    }

    public void OnClickedCloseButton()
    {
        if(_discardCardToHandEffector == null)
        {
            return;
        }

        _discardCardToHandEffector.Execute();
        _discardUI.CloseUI();
        OnDiscardUIVisibilityChanged?.Invoke(false);
    }

    public void OnClickedDiscardButton()
    {
        if(_discardCardToDiscardDeckEffector == null)
        {
            return;
        }

        _discardCardToDiscardDeckEffector.Execute();
        _discardUI.CloseUI();
        _turnManager.UpdateThrowAction(false);
        OnDiscardUIVisibilityChanged?.Invoke(false);
    }

    private void UpdateOpenButtonState(bool isActive)
        => _discardUI.UpdateOpenButtonState(isActive);

    private void UpdateDiscardCount(ActionData data)
        => _discardUI.UpdateDiscardButtonState(data.Current > 0);

    public void Dispose()
    {
        _turnManager.OnUpdatedThrowActionState -= UpdateOpenButtonState;
        _turnManager.OnUpdatedThrowCount -= UpdateDiscardCount;
    }
}
