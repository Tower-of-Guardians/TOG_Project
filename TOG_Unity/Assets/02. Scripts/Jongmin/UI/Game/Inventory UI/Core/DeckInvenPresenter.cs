public class DeckInvenPresenter
{
    protected readonly IDeckInvenUI _deckInvenUI;
    protected readonly ICardFactory<IDeckInvenCardUI> _deckInvenCardFactory;
    protected readonly CardContainer<IDeckInvenCardUI, DeckInvenCardPresenter> _deckInvenCardContainer;
    
    private ICardBehavior _cardBehavior;
    private ICardSelectionRequester _cardSelectionRequester;
    private ICardSelectionController _cardSelectionController;

    public DeckInvenPresenter(IDeckInvenUI deckInvenUI,
                              ICardFactory<IDeckInvenCardUI> deckInvenCardFactory,
                              CardContainer<IDeckInvenCardUI, DeckInvenCardPresenter> deckInvenCardContainer,
                              ICardBehavior cardBehavior)
    {
        _deckInvenUI = deckInvenUI;
        _deckInvenCardFactory = deckInvenCardFactory;
        _deckInvenCardContainer = deckInvenCardContainer;
        
        _cardBehavior = cardBehavior;
        _cardSelectionRequester = CreateSelectionRequester();
        _cardSelectionController = CreateSelectionController();
    }

    public virtual void OpenUI()
    {
        CreateCardsFromDataCenter();
        _deckInvenUI.OpenUI();
    }

    public virtual void CloseUI()
    {
        _deckInvenUI.CloseUI();
        RemoveAllCards();
    }
    
    protected virtual ICardSelectionRequester CreateSelectionRequester() => null;
    protected virtual ICardSelectionController CreateSelectionController() => null;

    protected void CreateCardsFromDataCenter()
    {
        DataCenter.Instance.SortUserCards(SortType.Grade);

        foreach (CardData cardData in DataCenter.Instance.userDeck)
        {
            CreateCard(cardData);
        }
    }

    protected void RemoveAllCards()
    {
        foreach (IDeckInvenCardUI cardUI in _deckInvenCardContainer.CardList)
        {
            _deckInvenCardFactory.Release(cardUI);
        }
        
        _deckInvenCardContainer.Clear();
    }
    
    private void CreateCard(CardData cardData)
    {
        IDeckInvenCardUI deckInvenCardUI = _deckInvenCardFactory.Create();
        DeckInvenCardPresenter deckInvenCardPresenter = new(deckInvenCardUI, 
                                                            cardData,
                                                            _cardBehavior,
                                                            _cardSelectionRequester,
                                                            _cardSelectionController);
        _deckInvenCardContainer.Add(deckInvenCardUI, deckInvenCardPresenter);
    }
}