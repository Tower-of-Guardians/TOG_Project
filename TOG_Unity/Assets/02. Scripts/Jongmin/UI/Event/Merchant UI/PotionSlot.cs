using System;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Jongmin
{
    public class PotionSlot : MonoBehaviour
    {
        [BigHeader("UI")]
        [SerializeField] private ImageView potionImage;
        [SerializeField] private ButtonView purchaseButton;
        [SerializeField] private Image soldOutImage;
        
        [Space(30f), BigHeader("Test")]
        [SerializeField] private int price = 40;
        
        private bool _isPurchased;

        public event Action Purchased;

        private void Awake()
        {
            purchaseButton.AddListener(Purchase);
        }

        public void Initialize(int gold)
        {
            _isPurchased = false;
            soldOutImage.gameObject.SetActive(false);
            potionImage.CanvasGroup.alpha = 1f;
            UpdateState(gold);
        }

        public void UpdateState(int gold)
        {
            if (_isPurchased)
            {
                return;
            }

            var canPurchase = gold >= price;
            UpdatePurchaseButton(canPurchase);
            UpdatePurchaseState(false);
        }

        private void Purchase()
        {
            if (_isPurchased)
            {
                return;
            }

            if (DataCenter.Instance.playerstate.money < price)
            {
                return;
            }
            
            _isPurchased = true;
            UpdatePurchaseState(true);
            DataCenter.Instance.SetMoney(-price);
            Purchased?.Invoke();
        }

        private void UpdatePurchaseButton(bool canPurchase)
        {
            purchaseButton.Label.text = canPurchase ? $"{price}" : $"<color=red>{price}</color>";
            purchaseButton.Button.interactable = canPurchase;
        }

        private void UpdatePurchaseState(bool isSoldOut)
        {
            potionImage.CanvasGroup.alpha = isSoldOut ? 0f : 1f;
            purchaseButton.CanvasGroup.alpha = isSoldOut ? 0f : 1f;
            soldOutImage.gameObject.SetActive(isSoldOut);
        }

        private void OnDestroy()
        {
            purchaseButton.RemoveAllListeners();
        }
    }
}