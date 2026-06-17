using System;
using UnityEngine;

namespace Jongmin
{
    public class InventoryDomain : MonoBehaviour
    {
        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private CardSortView cardSortView;
        [SerializeField] private InvenTabView invenTabView;
        
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private CardSortSystem cardSortSystem;
        
        [SerializeField] private CardInfoDomain cardInfoDomain;
        [SerializeField] private CardInvenDomain cardInvenDomain;

        public event Action<TabType> OnTabDeselected;
        
        public void Construct()
        {
            cardSortSystem.Construct(cardSortView);
            
            BindEvents();
        }

        public void BindEvents()
        {
            cardSortView.Bind(cardSortSystem);
            inventoryView.Bind(inventorySystem);
            invenTabView.Bind(this);

            cardSortSystem.RequestResetCriterion += cardInvenDomain.System.RefreshCardInventory;

            inventorySystem.RequestOpenView += HandleRequestOpenView;
            inventorySystem.RequestCloseView += HandleRequestCloseView;
        }

        public void ReleaseEvents()
        {
            cardSortSystem.RequestResetCriterion -= cardInvenDomain.System.RefreshCardInventory;
            
            inventorySystem.RequestOpenView -= HandleRequestOpenView;
            inventorySystem.RequestCloseView -= HandleRequestCloseView;
        }

        private void HandleRequestOpenView()
        {
            inventoryView.Show();
            cardSortSystem.Initialize();
            invenTabView.Show();
            invenTabView.Initialize();
            HandleOnClickedCardTab();
        }

        private void HandleRequestCloseView()
        {
            inventoryView.Hide();
            invenTabView.Hide();
            cardInvenDomain.System.CloseView();
        }

        public void HandleOnClickedCardTab()
        {
            cardInvenDomain.System.OpenView();
            // TODO: 카드 인벤토리 열기
            // TODO: 마법 인벤토리 닫기
            inventoryView.SetTitle("카드");
            OnTabDeselected?.Invoke(TabType.Card);
        }

        public void HandleOnClickedMagicTab()
        {
            cardInvenDomain.System.CloseView();
            // TODO: 카드 인벤토리 닫기
            // TODO: 마법 인벤토리 열기
            inventoryView.SetTitle("마법");
            OnTabDeselected?.Invoke(TabType.Magic);
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}