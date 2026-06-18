public interface ICardSelectionRequester
{
    bool RequestSelect(DeckInvenCardPresenter deckInvenCardPresenter);
    void RequestDeselect(DeckInvenCardPresenter deckInvenCardPresenter);
}