using System;
using System.Collections.Generic;
using System.Linq;
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
            merchantSystem.OpenSaleButtons();
        }

        public void HandleOnCanceledSale()
        {
            if (compactInvenDomain.System is MerchantInvenSystem merchantInvenSystem)
            {
                merchantInvenSystem.OnSelectionChanged -= HandleOnSelectedSlots;
            }

            compactInvenDomain.CloseView();
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.OpenMerchantView);
            shopSystem.OpenView();
            merchantSystem.CloseSaleButtons();
        }

        public void HandleOnClickedSell()
        {
            if (compactInvenDomain.System is not MerchantInvenSystem merchantInvenSystem ||
                merchantInvenSystem.SelectedCards.Count <= 0)
            {
                return;
            }

            var selectedCards = merchantInvenSystem.SelectedCards.ToArray();
            var totalPrice = selectedCards.Sum(selectedCard => selectedCard.price);

            DataCenter.Instance.SetMoney(totalPrice);
            shopDispenser.RefreshPurchaseStates();

            foreach (var selectedCard in selectedCards)
            {
                DataCenter.Instance.userDeck.Remove(selectedCard);
            }

            merchantInvenSystem.OnSelectionChanged -= HandleOnSelectedSlots;
            compactInvenDomain.CloseView();
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.CompletedSellCards);
            shopSystem.SetSaleButtonState(false);
            shopSystem.OpenView();
            merchantSystem.CloseSaleButtons();
        }

        private void BindEvents()
        {
            merchantView.Bind(this);
            shopView.Bind(this);

            merchantSystem.RequestOpenView += HandleRequestOpenView;
            merchantSystem.RequestCloseView += HandleRequestCloseView;
            shopDispenser.OnPurchasedCard += HandleOnPurchasedCard;
            shopDispenser.OnPurchasedHpPotion += HandleOnPurchasedHpPotion;
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
            shopDispenser.OnPurchasedCard -= HandleOnPurchasedCard;
            shopDispenser.OnPurchasedHpPotion -= HandleOnPurchasedHpPotion;
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
            if (selectedCards.Count <= 0)
            {
                merchantSystem.SetSellButtonInteractable(false);
                speechBubbleDomain.SetBubbleText(BubbleTriggerType.ClickedSellCard);
                return;
            }

            merchantSystem.SetSellButtonInteractable(true);

            if (selectedCards.Count == 1 && selectedCards[0].grade == 0)
            {
                speechBubbleDomain.SetBubbleText(BubbleTriggerType.SelectedMerchantSlot, false, 1);
                return;
            }

            var totalPrice = selectedCards.Sum(selectedCard => selectedCard.price);

            speechBubbleDomain.SetBubbleText(BubbleTriggerType.SelectedMerchantSlot, false, 0, totalPrice);
        }

        private void HandleOnPurchasedCard(CardData cardData)
        {
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.PurchasedCard);
            DataCenter.Instance.userDeck.Add(cardData);
        }

        private void HandleOnPurchasedHpPotion()
        {
            speechBubbleDomain.SetBubbleText(BubbleTriggerType.PurchasedHpPotion);

            var maxHp = DataCenter.Instance.playerstate.maxhp;
            var targetHp = maxHp * 0.2f;
            DataCenter.Instance.SetPlayerHP((int)targetHp);
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}
