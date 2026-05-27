using UnityEngine;

public class InventoryTabUI : MonoBehaviour, IInventoryTabUI
{
    [Header("UI References")]
    [SerializeField] private TabButton cardTabButton;
    [SerializeField] private TabButton magicTabButton;

    public void Construct(InventoryTabPresenter inventoryTabPresenter)
    {
        cardTabButton.Construct(inventoryTabPresenter);
        magicTabButton.Construct(inventoryTabPresenter);
        
        cardTabButton.OnClick += inventoryTabPresenter.OnClickedCardTab;
        magicTabButton.OnClick += inventoryTabPresenter.OnClickedMagicTab;
    }

    public void Initialize()
    {
        cardTabButton.SelectButton();
    }
}
