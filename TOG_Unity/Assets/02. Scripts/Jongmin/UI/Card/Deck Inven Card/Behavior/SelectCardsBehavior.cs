public class SelectCardsBehavior : ICardBehavior
{
    public void OnClick(DeckInvenCardPresenter deckInvenCardPresenter)
        => deckInvenCardPresenter.RequestToggleSelect();

    public void OnPointerEnter(DeckInvenCardPresenter deckInvenCardPresenter) {}
    public void OnPointerExit(DeckInvenCardPresenter deckInvenCardPresenter) {}
}
