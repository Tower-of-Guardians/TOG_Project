public class InventoryPresenter
{
    private readonly IInventoryUI _inventoryUI;
    private readonly InventoryTabPresenter _tabPresenter;
    private readonly InventorySortPresenter _sortPresenter;

    private bool _isActive;

    public InventoryPresenter(IInventoryUI inventoryUI,
                              InventoryTabPresenter tabPresenter,
                              InventorySortPresenter sortPresenter)
    {
        _inventoryUI = inventoryUI;
        _tabPresenter = tabPresenter;
        _sortPresenter = sortPresenter;

        tabPresenter.OnActivateTab += _inventoryUI.UpdateTitle;
        
        _inventoryUI.Construct(this);
    }

    public void ToggleUI()
    {
        _isActive = !_isActive;
        
        if (_isActive)
        {
            _inventoryUI.OpenUI();
            _tabPresenter.Initialize();
            _sortPresenter.Initialize();
        }
        else
        {
            _inventoryUI.CloseUI();
        }
    }
}
