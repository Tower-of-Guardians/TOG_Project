using System.Collections.Generic;

public interface ISeriesView
{
    void OpenUI();
    void CloseUI();

    void UpdateSeries(List<CardData> card_data_list);
}