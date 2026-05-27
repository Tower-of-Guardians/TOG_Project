public interface IInventorySortUI
{
    void Construct(InventorySortPresenter inventorySortPresenter);
    void UpdateSortLabel(string sortText);
    void ResetCriterion();
}