using System.Collections.Generic;
using VContainer.Unity;

public class DeckPresenter : IInitializable
{
    private readonly IDeckUI _deckUI;
    private readonly CardContainer<IDeckCardUI, DeckCardPresenter> _deckCardContainer;
    private readonly ICardFactory<IDeckCardUI> _deckCardFactory;

    public DeckPresenter(IDeckUI deckUI,
                         CardContainer<IDeckCardUI, DeckCardPresenter> deckCardContainer,
                         ICardFactory<IDeckCardUI> deckCardFactory)
    {
        _deckUI = deckUI;
        _deckCardContainer = deckCardContainer;
        _deckCardFactory = deckCardFactory;
    }
    
    public void Initialize()
    {
        _deckUI.Construct(this);
        
        GameData.Instance.DeckChange += UpdateCardCount;
        GameData.Instance.InvokeDeckCountChange(DeckType.Draw);
        GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
    }
    
    public void OpenUI(DeckType deckType)
    {
        string titleName = GetTitleName(deckType);
        
        _deckUI.OpenUI();
        _deckUI.UpdateUI(titleName);

        IEnumerable<BattleCardData> battleCardDataList = GameData.Instance.GetDeckDatas(deckType);
        foreach (BattleCardData battleCardData in battleCardDataList)
        {
            CreateCard(battleCardData);
        }
    }
    
    public void CloseUI()
    {
        IEnumerable<IDeckCardUI> deckCardUIList = _deckCardContainer.CardList;
        foreach (IDeckCardUI deckCardUI in deckCardUIList)
        {
            RemoveCard(deckCardUI);
        }
        
        _deckCardContainer.Clear();
        _deckUI.CloseUI();
    }

    private void UpdateCardCount(DeckType deckType, int count)
    {
        switch(deckType)
        {
            case DeckType.Draw:
                _deckUI.UpdateDrawCardCount(count);
                break;
            
            case DeckType.Throw:
                _deckUI.UpdateThrowCardCount(count);
                break;
        }
    }

    private string GetTitleName(DeckType deckType)
        => deckType switch
           { 
                DeckType.Draw   => "사용하지 않은 카드 더미",
                DeckType.Throw  => "사용한 카드 더미",
                _               => string.Empty
           };

    private void CreateCard(BattleCardData cardData)
    {
        IDeckCardUI deckCardUI = _deckCardFactory.Create();
        DeckCardPresenter deckCardPresenter = new(deckCardUI, cardData);
        
        _deckCardContainer.Add(deckCardUI, deckCardPresenter);
    }

    private void RemoveCard(IDeckCardUI cardUI)
        => _deckCardFactory.Release(cardUI);
}