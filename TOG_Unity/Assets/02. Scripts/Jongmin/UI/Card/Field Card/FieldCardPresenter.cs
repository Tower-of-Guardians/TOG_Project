public class FieldCardPresenter : CardPresenter
{
    private readonly IFieldCardUI _cardUI;

    public CardData CardData => BattleCardData.data;

    public FieldCardPresenter(IFieldCardUI cardUI,
                              BattleCardData battleCardData,
                              bool isAtk)
    {
        _cardUI = cardUI;
        BattleCardData = battleCardData;

        _cardUI.UpdateUI(CardData, isAtk);
    }

    /// <summary>
    /// 각 공격력/방어력 능력치 잠금 표시를 토글합니다.
    /// </summary>
    public void ToggleLock()
        => _cardUI.ToggleLock();
}
