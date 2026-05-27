public class SeriesPresenter
{
    private readonly ISeriesView m_view;

    public SeriesPresenter(ISeriesView view)
        => m_view = view;

    public void OpenUI(CardData card_data)
    {
        m_view.OpenUI();

        var series_list = DataCenter.Instance.GetSeriesCards(card_data.id);
        m_view.UpdateSeries(series_list);
    }

    public void CloseUI()
        => m_view.CloseUI();
}
