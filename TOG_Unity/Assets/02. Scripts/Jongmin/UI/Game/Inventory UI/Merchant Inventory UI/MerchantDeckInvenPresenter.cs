using System;
using System.Collections.Generic;

public sealed class MerchantDeckInvenPresenter : DeckInvenPresenter, ICardSelectionRequester
{
    private ShopPresenter _shopPresenter;
    private readonly MerchantDialogueBubblePresenter _dialogueBubblePresenter;

    private readonly HashSet<DeckInvenCardPresenter> _selectedCardSet = new();
    private const int MaxSelectCount = 3;

    public event Action<int, int, int> OnSelectedCardsChanged;

    public MerchantDeckInvenPresenter(IDeckInvenUI deckInvenUI, 
                                      ICardFactory<IDeckInvenCardUI> deckInvenCardFactory,
                                      CardContainer<IDeckInvenCardUI, DeckInvenCardPresenter> deckInvenCardContainer,
                                      ICardBehavior behavior,
                                      MerchantDialogueBubblePresenter dialogueBubblePresenter) 
        : base(deckInvenUI, deckInvenCardFactory, deckInvenCardContainer, behavior)
    {
        _dialogueBubblePresenter = dialogueBubblePresenter;
        _deckInvenUI.Construct(this);
    }

    public void Inject(ShopPresenter shopPresenter)
        => _shopPresenter = shopPresenter;

    public override void OpenUI()
    {
        _dialogueBubblePresenter.OpenUI(this);
        AlertUpdateSelectedCards();
        base.OpenUI();
    }

    public override void CloseUI()
    {
        _dialogueBubblePresenter.CloseUI(this);
        base.CloseUI();
    }

    public void OnClickedSale()
    {
        foreach (var cardPresenter in _selectedCardSet)
        {
            DataCenter.Instance.userDeck.Remove(cardPresenter.CardData);
        }

        int totalMoney = GetTotalSalePrice();
        DataCenter.Instance.playerstate.money += totalMoney;

        _shopPresenter.ToggleSaleButton(false);

        RemoveAllCards();
        CreateCardsFromDataCenter();
        OnClickedBack();
    }

    public void OnClickedBack()
    {
        _shopPresenter.FadeUpUI();
        CloseUI();
    }

    protected override ICardSelectionRequester CreateSelectionRequester()
        => this;
    
    private void AlertUpdateSelectedCards()
        => OnSelectedCardsChanged?.Invoke(_selectedCardSet.Count, MaxSelectCount, GetTotalSalePrice());

    private int GetTotalSalePrice()
    {
        int totalMoney = 0;
        
        foreach (DeckInvenCardPresenter cardPresenter in _selectedCardSet)
        {
            totalMoney += cardPresenter.CardData.price;
        }

        totalMoney = (int)(totalMoney * 0.8f);
        return totalMoney;
    }

    public bool RequestSelect(DeckInvenCardPresenter deckInvenCardPresenter)
    {
        if (_selectedCardSet.Count >= MaxSelectCount)
        {
            return false;
        }
        
        _selectedCardSet.Add(deckInvenCardPresenter);
        deckInvenCardPresenter.SetSelected(true);
        AlertUpdateSelectedCards();

        return true;
    }

    public void RequestDeselect(DeckInvenCardPresenter deckInvenCardPresenter)
    {
        if (!_selectedCardSet.Remove(deckInvenCardPresenter))
        {
            return;
        }
        
        deckInvenCardPresenter.SetSelected(false);
        AlertUpdateSelectedCards();
    }
}
