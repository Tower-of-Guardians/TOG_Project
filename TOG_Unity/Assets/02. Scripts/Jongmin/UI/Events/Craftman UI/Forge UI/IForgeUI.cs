public interface IForgeUI : IOpenableUI
{
    void Construct(ForgePresenter forgePresenter);
    void ToggleCloseButton(bool isActive);
    void ToggleButtonGroup(bool isActive);
}