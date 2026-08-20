namespace Jongmin
{
    public interface ITooltipProvider
    {
        TooltipContent GetTooltipContent();
        bool CanShowTooltip { get; }
    }
}