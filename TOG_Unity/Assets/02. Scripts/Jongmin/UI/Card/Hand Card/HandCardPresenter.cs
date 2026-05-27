public class HandCardPresenter : CardPresenter
{
    private readonly IHandCardUI _cardUI;

    public CardData CardData => BattleCardData.data;

    public HandCardPresenter(IHandCardUI cardUI,
                             BattleCardData battleCardData)
    {
        _cardUI = cardUI;
        BattleCardData = battleCardData;

        _cardUI.UpdateUI(CardData);
    }
}
