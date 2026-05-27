using VContainer;

public class ResultDeckInvenPresenter : DeckInvenPresenter
{
    public ResultDeckInvenPresenter([Key(DeckInvenType.Result)]IDeckInvenUI deckInvenUI, 
                                    [Key(DeckInvenType.Result)]ICardFactory<IDeckInvenCardUI> deckInvenCardFactory, 
                                    [Key(DeckInvenType.Result)]CardContainer<IDeckInvenCardUI, DeckInvenCardPresenter> deckInvenCardContainer, 
                                    ICardBehavior cardBehavior) 
        : base(deckInvenUI, deckInvenCardFactory, deckInvenCardContainer, cardBehavior)
    { }
}