public interface ITooltipView
{
    void OpenUI();
    void UpdateUI(TooltipData tooltip_string);
    void CloseUI();
}