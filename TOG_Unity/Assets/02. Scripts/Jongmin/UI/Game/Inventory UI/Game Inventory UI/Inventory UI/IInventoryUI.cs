public interface IInventoryUI : IOpenableUI
{
    void Construct(InventoryPresenter inventoryPresenter);
    void UpdateTitle(string titleText);
}