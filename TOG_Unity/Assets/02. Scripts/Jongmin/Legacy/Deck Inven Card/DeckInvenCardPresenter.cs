public class DeckInvenCardPresenter : CardPresenter
{
    protected readonly IDeckInvenCardUI _deckInvenCardUI;
    
    protected readonly ICardBehavior _cardBehavior;
    private readonly ICardSelectionRequester _cardSelectionRequester;
    private readonly ICardSelectionController _cardSelectionController;

    private bool _isSelected;
    
    public CardData CardData { get; private set; }

    public DeckInvenCardPresenter(IDeckInvenCardUI deckInvenCardUI,
                                  CardData cardData,
                                  ICardBehavior cardBehavior,
                                  ICardSelectionRequester cardSelectionRequester,
                                  ICardSelectionController cardSelectionController)
    {
        _deckInvenCardUI = deckInvenCardUI;
        _deckInvenCardUI.Construct(this);
        
        _cardBehavior = cardBehavior;
        _cardSelectionRequester = cardSelectionRequester;
        _cardSelectionController = cardSelectionController;
        
        CardData = cardData;
        _deckInvenCardUI.UpdateUI(CardData);
    }

    public virtual void OnPointerClick()
        => _cardBehavior?.OnClick(this);

    public virtual void OnPointerEnter() 
        => _cardBehavior?.OnPointerEnter(this);

    public virtual void OnPointerExit()
        => _cardBehavior?.OnPointerExit(this);
    

    /// <summary>
    /// 카드가 선택되어 있지 않다면 선택, 선택되어 있다면 선택 해제를 요청합니다.
    /// </summary>
    public void RequestToggleSelect()
    {
        if (_isSelected)
        {
            _cardSelectionRequester?.RequestDeselect(this);
        }
        else
        {
            _cardSelectionRequester?.RequestSelect(this);
        }
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        _deckInvenCardUI.ShowHighlight(isSelected);
    }

    public void SelectCard()
        => _cardSelectionController.Select(CardData);
    
    public void RequestDeselect()
        => _cardSelectionRequester?.RequestDeselect(this);
}