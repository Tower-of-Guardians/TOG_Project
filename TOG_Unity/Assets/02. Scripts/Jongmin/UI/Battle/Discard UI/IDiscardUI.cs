public interface IDiscardUI : IOpenableUI
{
    void UpdateOpenButtonState(bool isOpenButtonActive);
    void UpdateDiscardButtonState(bool isDiscardButtonActive);

    void TogglePreview(bool isActive);
}