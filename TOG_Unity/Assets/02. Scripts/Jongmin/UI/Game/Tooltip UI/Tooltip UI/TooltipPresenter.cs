public class TooltipPresenter
{
    private readonly ITooltipView m_view;

    public TooltipPresenter(ITooltipView view)
    {
        m_view = view;
    }

    public void OpenUI(IDescriptable descriptable)
    {
        m_view.OpenUI();
        UpdateUI(descriptable);
    }

    public void UpdateUI(IDescriptable descriptable)
    {
        m_view.UpdateUI(descriptable.GetTooltipData());
    }

    public void CloseUI()
    {
        m_view.CloseUI();
    }
}