using System;

public class InventoryTabPresenter
{
    private readonly IInventoryTabUI _inventoryTabUI;
    private readonly CardInventoryUI _cardInventoryUI;
    
    public event Action<TabType> OnDeselectTab;
    public event Action<string> OnActivateTab;

    public InventoryTabPresenter(IInventoryTabUI inventoryTabUI,
                                 CardInventoryUI cardInventoryUI)
    {
        _inventoryTabUI = inventoryTabUI;
        _cardInventoryUI = cardInventoryUI;

        _inventoryTabUI.Construct(this);
    }

    public void Initialize()
    {
        _inventoryTabUI.Initialize();
        OnClickedCardTab();
    }

    public void OnClickedCardTab()
    {
        _cardInventoryUI.OpenPanel();
        // TODO: 마법 인벤토리 닫기
        OnDeselectTab?.Invoke(TabType.Card);
        OnActivateTab?.Invoke("카드");
    }

    public void OnClickedMagicTab()
    {
        _cardInventoryUI.ClosePanel();
        // TODO: 마법 인벤토리 열기
        OnDeselectTab?.Invoke(TabType.Magic);
        OnActivateTab?.Invoke("마법");
    }
}
