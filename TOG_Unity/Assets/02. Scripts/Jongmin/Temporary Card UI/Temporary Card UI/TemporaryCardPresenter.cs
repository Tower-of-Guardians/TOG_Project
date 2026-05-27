public class TemporaryCardPresenter : CardPresenter
{
    private readonly ICardUI m_view;

    public TemporaryCardPresenter(ICardUI view,
                                  BattleCardData card_data)
    {
        m_view = view;
        BattleCardData = card_data;

        m_view.UpdateUI(card_data.data);
    }
}
