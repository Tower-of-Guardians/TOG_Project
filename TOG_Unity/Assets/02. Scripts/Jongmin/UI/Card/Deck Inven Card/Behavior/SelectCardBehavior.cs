public class SelectCardBehavior : ICardBehavior
{
    public void OnClick(DeckInvenCardPresenter deckInvenCardPresenter)
        => deckInvenCardPresenter.SelectCard();

    public void OnPointerEnter(DeckInvenCardPresenter deckInvenCardPresenter) {}
    public void OnPointerExit(DeckInvenCardPresenter deckInvenCardPresenter) {}

}