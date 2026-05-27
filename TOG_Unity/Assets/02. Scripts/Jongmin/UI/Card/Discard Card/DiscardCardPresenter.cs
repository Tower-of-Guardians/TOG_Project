public class DiscardCardPresenter : CardPresenter
{
    private readonly IDiscardCardUI _cardUI;

    public CardData CardData => BattleCardData.data;

    public DiscardCardPresenter(IDiscardCardUI cardUI, 
                                BattleCardData battleCardData)
    {
        _cardUI = cardUI;
        BattleCardData = battleCardData;

        _cardUI.UpdateUI(CardData);
    }
}