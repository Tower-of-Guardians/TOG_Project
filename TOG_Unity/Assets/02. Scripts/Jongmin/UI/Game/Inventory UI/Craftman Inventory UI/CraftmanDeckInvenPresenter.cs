using System;

public class CraftmanDeckInvenPresenter : DeckInvenPresenter, ICardSelectionController
{
    public event Action<CardData> OnCardSelected;
    public event Action OnInvenOpened;
    public event Action OnInvenClosed;
    public event Action OnRequestedDefaultBubble;

    public CraftmanDeckInvenPresenter(IDeckInvenUI deckInvenUI, 
                                      ICardFactory<IDeckInvenCardUI> deckInvenCardFactory,
                                      CardContainer<IDeckInvenCardUI, DeckInvenCardPresenter> deckInvenCardContainer,
                                      ICardBehavior behavior) 
        : base(deckInvenUI, deckInvenCardFactory, deckInvenCardContainer, behavior)
    {
        _deckInvenUI.Construct(this);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        OnInvenOpened?.Invoke();
        OnRequestedDefaultBubble?.Invoke();
    }

    public override void CloseUI()
    {
        base.CloseUI();
        OnInvenClosed?.Invoke();
    }

    public void FadeUI(bool isActive)
    {
        if (isActive)
        {
            OpenUI();
        }
        else
        {
            base.CloseUI();
        }
    }

    public void Select(CardData cardData)
    {
        OnCardSelected?.Invoke(cardData);
        FadeUI(false);
    }

    protected override ICardSelectionController CreateSelectionController()
        => this;
}
