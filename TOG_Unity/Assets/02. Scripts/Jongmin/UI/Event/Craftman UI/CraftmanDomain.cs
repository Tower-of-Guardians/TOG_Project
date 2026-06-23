using JxModule;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class CraftmanDomain : MonoBehaviour
    {
        [SerializeField] private CraftmanView craftmanView;
        [SerializeField] private ForgeView forgeView;
        [SerializeField] private CraftmanSystem craftmanSystem;
        [SerializeField] private ForgeSystem forgeSystem;
        [SerializeField] private CompactInvenDomain compactInvenDomain;

        private DataTable _forgeDataTable;

        [Button("Test")]
        public void OpenView()
        {
            craftmanSystem.OpenView();
        }
        
        public void Construct()
        {
            _forgeDataTable = DataTableManager.FindTable<ForgeDataTableRow>("DT_Forge");
            
            craftmanSystem.Construct(craftmanView);
            forgeSystem.Construct(forgeView, _forgeDataTable);

            BindEvents();
        }
        
        public void BindEvents()
        {
            forgeView.Bind(this);
            
            craftmanSystem.RequestOpenView += HandleRequestOpenView;
            craftmanSystem.RequestCloseView += HandleRequestCloseView;
        }
        
        public void ReleaseEvents()
        {
            craftmanSystem.RequestOpenView -= HandleRequestOpenView;
            craftmanSystem.RequestCloseView -= HandleRequestCloseView;
        }

        private void HandleRequestOpenView()
        {
            forgeSystem.CloseView();
            compactInvenDomain.OpenView(CompactInvenType.Craftman);

            if (compactInvenDomain.System is CraftmanInvenSystem craftmanInvenSystem)
            {
                craftmanInvenSystem.OnSelectedSlot += HandleOnSelectedSlot;
            }
        }

        private void HandleRequestCloseView()
        {
            if (compactInvenDomain.System is CraftmanInvenSystem craftmanInvenSystem)
            {
                craftmanInvenSystem.OnSelectedSlot -= HandleOnSelectedSlot;
            }
            
            forgeSystem.CloseView();
            compactInvenDomain.CloseView();
        }

        public void HandleOnClickedAtkUpgrade()
        {
            forgeSystem.UpgradeAtkRate();
        }

        public void HandleOnClickedBothUpgrade()
        {
            forgeSystem.UpgradeBothRate();
        }

        public void HandleOnClickedDefUpgrade()
        {
            forgeSystem.UpgradeDefRate();
        }

        public void HandleOnCanceledUpgrade()
        {
            forgeSystem.CloseView();
            compactInvenDomain.OpenView(CompactInvenType.Craftman);
        }

        public void HandleOnRequestClose()
        {
            craftmanSystem.CloseView();
        }

        private void HandleOnSelectedSlot(CardData cardData)
        {
            compactInvenDomain.CloseView(false);
            forgeSystem.OpenView(cardData);
        }
        
        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}