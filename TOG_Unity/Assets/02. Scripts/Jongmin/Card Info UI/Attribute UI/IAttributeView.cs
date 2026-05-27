using System.Collections.Generic;

public interface IAttributeView
{
    void OpenUI();
    void CloseUI();

    void UpdateCard(CardData card_data);
    void UpdateSynergy(List<string> synergy_desc_list);
}