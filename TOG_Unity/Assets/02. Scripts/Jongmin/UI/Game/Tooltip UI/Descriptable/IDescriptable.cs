public interface IDescriptable
{
    void Inject(TooltipPresenter tooltip_presenter);
    TooltipData GetTooltipData();
}