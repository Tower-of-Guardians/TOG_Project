using System;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class ShopSlot : MonoBehaviour
    {
        [Header("Card")]
        [SerializeField] private Card card;
        [SerializeField] private ButtonView purchaseButton;
        
        [Header("Disable Group")]
        [SerializeField] private Image soldOutImage;

        private bool _isPurchased;

        public Card Card => card;
        public event Action<CardData> Purchased;

        private void Awake()
        {
            purchaseButton.AddListener(Purchase);
        }

        public void Initialize(CardData cardData, int gold)
        {
            _isPurchased = false;
            soldOutImage.gameObject.SetActive(false);
            Card.SetCardData(cardData);
            UpdateState(gold);
        }

        public void UpdateState(int gold)
        {
            if (_isPurchased)
            {
                return;
            }

            var canPurchase = gold >= card.CardData.price;
            UpdatePurchaseButton(canPurchase);
            UpdatePurchaseState(false);
        }

        private void Purchase()
        {
            if (_isPurchased)
            {
                return;
            }

            if (DataCenter.Instance.playerstate.money < Card.CardData.price)
            {
                return;
            }
            
            _isPurchased = true;
            UpdatePurchaseState(true);
            DataCenter.Instance.SetMoney(-Card.CardData.price);
            Purchased?.Invoke(Card.CardData);
        }

        private void UpdatePurchaseButton(bool canPurchase)
        {
            purchaseButton.Label.text = canPurchase ? $"{Card.CardData.price}" : $"<color=red>{Card.CardData.price}</color>";
            purchaseButton.Button.interactable = canPurchase;
        }

        private void UpdatePurchaseState(bool isSoldOut)
        {
            Card.View.CanvasGroup.alpha = isSoldOut ? 0f : 1f;
            purchaseButton.CanvasGroup.alpha = isSoldOut ? 0f : 1f;
            soldOutImage.gameObject.SetActive(isSoldOut);
        }

        private void OnDestroy()
        {
            purchaseButton.RemoveAllListeners();
        }
    }
}