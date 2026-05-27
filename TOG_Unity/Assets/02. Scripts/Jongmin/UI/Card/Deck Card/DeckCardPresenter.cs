public class DeckCardPresenter : CardPresenter
{
    private readonly IDeckCardUI _deckCardUI;

    public CardData CardData => BattleCardData.data;
    
    public DeckCardPresenter(IDeckCardUI deckCardUI,
                             BattleCardData battleCardData)
    {
        _deckCardUI = deckCardUI;
        BattleCardData = battleCardData;

        _deckCardUI?.UpdateUI(CardData);
    }
}