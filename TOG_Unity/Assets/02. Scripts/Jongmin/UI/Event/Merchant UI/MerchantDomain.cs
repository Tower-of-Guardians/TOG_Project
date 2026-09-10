using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jongmin
{
    public class MerchantDomain : MonoBehaviour
    {
        [SerializeField] private MerchantView merchantView;
        [SerializeField] private ShopView shopView;
        [SerializeField] private MerchantSystem merchantSystem;
        [SerializeField] private ShopSystem shopSystem;
        [SerializeField] private ShopDispenser shopDispenser;
        [SerializeField] private CompactInvenDomain compactInvenDomain;
        [SerializeField] private SpeechBubbleDomain speechBubbleDomain;

        private bool _isClosing;
        
        public bool IsOpen { get; private set; }
        public event Action ViewClosed;
        
        public void Construct()
        {
            merchantSystem.Construct(merchantView);
            shopSystem.Construct(shopView);

            BindEvents();
        }

        public void OpenView()
        {
            if (IsOpen)
            {
                return;
            }

            if (!shopDispenser.Initialize())
            {
                return;
            }
            
            IsOpen = true;
            merchantSystem.OpenView();
        }

        public void CloseView()
        {
            if (!IsOpen || _isClosing)
            {
                return;
            }
            
            _isClosing = true;
            merchantSystem.CloseView(() =>
            {
                IsOpen = false;
                _isClosing = false;
                ViewClosed?.Invoke();
            });
        }
        
        public void HandleOnClickedSale()
        {
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.ClickedSellCard);
            shopSystem.CloseView();
            compactInvenDomain.OpenView(CompactInvenType.Merchant);
            BindMerchantInvenSystem();
        }

        private void BindEvents()
        {
            merchantView.Bind(this);
            shopView.Bind(this);

            merchantSystem.RequestOpenView += HandleRequestOpenView;
            merchantSystem.RequestCloseView += HandleRequestCloseView;
        }

        private void ReleaseEvents()
        {
            if (merchantView != null)
            {
                merchantView.ReleaseEvents();
            }

            if (shopView != null)
            {
                shopView.ReleaseEvents();
            }

            merchantSystem.RequestOpenView -= HandleRequestOpenView;
            merchantSystem.RequestCloseView -= HandleRequestCloseView;
        }

        private void HandleRequestOpenView()
        {
            speechBubbleDomain.OpenView(SpeechBubbleType.Merchant);
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.OpenMerchantView);

            compactInvenDomain.CloseView();
            shopSystem.SetSaleButtonState(true);
            shopSystem.OpenView();
        }

        private void HandleRequestCloseView()
        {
            if (compactInvenDomain.System is MerchantInvenSystem merchantInvenSystem)
            {
                merchantInvenSystem.OnSelectionChanged -= HandleOnSelectedSlots;
            }

            compactInvenDomain.CloseView();
            shopSystem.CloseView();
        }

        private void BindMerchantInvenSystem()
        {
            if (compactInvenDomain.System is MerchantInvenSystem merchantInvenSystem)
            {
                merchantInvenSystem.OnSelectionChanged -= HandleOnSelectedSlots;
                merchantInvenSystem.OnSelectionChanged += HandleOnSelectedSlots;
            }
        }

        private void HandleOnSelectedSlots(IReadOnlyList<CardData> selectedCards)
        {
            if (selectedCards.Count > 0)
            {
                speechBubbleDomain.SetBubbleText(BubbleTriggerType.SelectedMerchantSlot);
            }
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}
