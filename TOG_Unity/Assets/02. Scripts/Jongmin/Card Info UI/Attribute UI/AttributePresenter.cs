public class AttributePresenter
{
    private readonly IAttributeView m_view;

    public AttributePresenter(IAttributeView view)
        => m_view = view;

    public void OpenUI(CardData card_data)
    {
        m_view.OpenUI();
        m_view.UpdateCard(card_data);
        // TODO: 시너지 정보 받아서 UpdateSynergy 호출하기기
    }

    public void CloseUI()
        => m_view.CloseUI();
}
