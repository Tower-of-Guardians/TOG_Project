using System;
using VContainer.Unity;

public abstract class FieldPresenter : ICardDropTarget<IFieldCardUI>, IInitializable
{
    protected readonly IFieldUI _fieldUI;
    protected readonly CardContainer<IFieldCardUI, FieldCardPresenter> _fieldCardContainer;
    protected readonly ICardFactory<IFieldCardUI> _fieldCardFactory;
    protected readonly FieldCardLayoutController _fieldCardLayout;
    
    private readonly TurnManager _turnManager;

    protected readonly bool _isAtk;
    public bool IsAtk => _isAtk;
    
    const int MaxCardCount = 4;
    protected bool CanAdd => _fieldCardContainer.CardList.Count < MaxCardCount; 
    public bool CanInteraction { get; private set; } = true;
    
    public IFieldCardUI HoverCard { get; set; }
     
    public FieldPresenter(IFieldUI fieldUI,
                          CardContainer<IFieldCardUI, FieldCardPresenter> fieldCardContainer,
                          ICardFactory<IFieldCardUI> fieldCardFactory,
                          FieldCardLayoutController fieldCardLayout,
                          bool isAtk,
                          TurnManager turnManager)
    {
        _fieldUI = fieldUI;
        _fieldCardContainer = fieldCardContainer;
        _fieldCardFactory = fieldCardFactory;
        _fieldCardLayout = fieldCardLayout;
        _isAtk = isAtk;
        _turnManager = turnManager;
    }

    public void Initialize()
        => _fieldCardLayout.Construct(_fieldCardContainer, this);
    
    /// <summary>
    /// battleCardData에 해당하는 카드를 생성합니다.
    /// </summary>
    public virtual void CreateCard(BattleCardData battleCardData)
    {
        IFieldCardUI fieldCardUI = _fieldCardFactory.Create();
        FieldCardPresenter fieldCardPresenter = new(fieldCardUI, battleCardData, _isAtk);

        _fieldCardContainer.Add(fieldCardUI, fieldCardPresenter);
        _fieldCardLayout.UpdateLayout(false, false, false);
        _turnManager.UpdateActionCount(1);
    }

    /// <summary>
    /// 해당 카드를 삭제하면서 레이아웃 재조정 여부를 통해 레이아웃을 재조정합니다.
    /// </summary>
    public void RemoveCard(IFieldCardUI cardUI, bool dontUseThis = true)
    {
        if(_fieldCardContainer.Remove(cardUI))
        {
            _fieldCardFactory.Release(cardUI);
            _turnManager.UpdateActionCount(-1);
        }
    }

    public bool TryRemoveCard(BattleCardData battleCardData)
    {
        if(!_fieldCardContainer.TryGetUI(battleCardData, out IFieldCardUI cardUI))
        {
            return false;
        }

        RemoveCard(cardUI);
        return true;
    }

    /// <summary>
    /// 카드를 통해 카드의 데이터와 탐색 성공 여부를 반환합니다.
    /// </summary>
    public bool TryGetBattleCardData(IFieldCardUI cardUI, out BattleCardData battleCardData)
        => _fieldCardContainer.TryGetCardData(cardUI, out battleCardData);

    public void TogglePreview(bool isActive)
    {
        if(!CanInteraction)
        {
            return;
        }
        
        if(isActive && CanAdd)
        {
            _fieldUI.TogglePreview(true);
            _fieldCardLayout.UpdateLayout(true);
        }
        else if(!isActive)
        {
            _fieldCardLayout.UpdateLayout(false);
            _fieldUI.TogglePreview(false);
        }
    }

    public void UpdateInteraction(bool isActive)
        => CanInteraction = !isActive;
}
