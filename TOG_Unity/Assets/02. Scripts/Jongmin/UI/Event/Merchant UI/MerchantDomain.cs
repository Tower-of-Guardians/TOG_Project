using System;
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
            
            compactInvenDomain.CloseView();
            merchantSystem.OpenView();
            shopSystem.SetSaleButtonState(true);
            shopSystem.OpenView();
        }

        public void CloseView()
        {
            if (!IsOpen || _isClosing)
            {
                return;
            }
            
            _isClosing = true;
            
            compactInvenDomain.CloseView();
            shopSystem.CloseView();
            merchantSystem.CloseView(() =>
            {
                IsOpen = false;
                _isClosing = false;
                ViewClosed?.Invoke();
            });
        }
        
        public void HandleOnClickedSale()
        {
            shopSystem.CloseView();
            compactInvenDomain.OpenView(CompactInvenType.Merchant);
        }

        private void BindEvents()
        {
            merchantView.Bind(this);
            shopView.Bind(this);
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
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}