public interface IDeckInvenCardUI : ICardUI
{
    void Construct(DeckInvenCardPresenter deckInvenCardPresenter);
    void ShowHighlight(bool isActive);
}
