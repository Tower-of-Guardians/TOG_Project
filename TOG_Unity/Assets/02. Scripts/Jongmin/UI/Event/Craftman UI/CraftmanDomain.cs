using System;
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
        [SerializeField] private SpeechBubbleDomain speechBubbleDomain;

        private DataTable _forgeDataTable;

        public void OnGUI()
        {
            if (GUI.Button(new Rect(new Vector2(1800, 100), new Vector2(100, 50)), "Smithy"))
            {
                OpenView();
            }
        }

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
            speechBubbleDomain.OpenView(SpeechBubbleType.Craftman);
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.OpenCraftmanView);
            
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
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.CompletedUpgradeCard);
        }

        public void HandleOnClickedBothUpgrade()
        {
            forgeSystem.UpgradeBothRate();
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.CompletedUpgradeCard);
        }

        public void HandleOnClickedDefUpgrade()
        {
            forgeSystem.UpgradeDefRate();
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.CompletedUpgradeCard);
        }

        public void HandleOnCanceledUpgrade()
        {
            forgeSystem.CloseView();
            compactInvenDomain.OpenView(CompactInvenType.Craftman);
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.OpenCraftmanView);
        }

        public void HandleOnRequestClose()
        {
            craftmanSystem.CloseView();
        }

        private void HandleOnSelectedSlot(CardData cardData)
        {
            compactInvenDomain.CloseView(false);
            forgeSystem.OpenView(cardData);
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.SelectedCraftmanSlot);
        }
        
        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}