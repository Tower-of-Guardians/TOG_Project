using System;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class ForgeView : ViewBase
    {
        [BigHeader("UI")]
        [SerializeField] private Card card;
        [SerializeField] private CanvasGroup upgradeButtonGroup;
        [SerializeField] private ButtonView atkUpgradeButton;
        [SerializeField] private ButtonView bothUpgradeButton;
        [SerializeField] private ButtonView defUpgradeButton;
        [SerializeField] private ButtonView cancelButton;
        [SerializeField] private ButtonView closeButton;
        
        [Space(30f)]
        [BigHeader("Effect")]
        [SerializeField] private ForgeVisualEffect forgeVisualEffect;

        private Vector2 _originAnchoredPosition;

        public Card Card => card;

        private void Awake()
        {
            _originAnchoredPosition = RectTransform.anchoredPosition;
            forgeVisualEffect ??= GetComponent<ForgeVisualEffect>();
        }

        public void Bind(CraftmanDomain domain)
        {
            atkUpgradeButton.AddListener(domain.HandleOnClickedAtkUpgrade);
            bothUpgradeButton.AddListener(domain.HandleOnClickedBothUpgrade);
            defUpgradeButton.AddListener(domain.HandleOnClickedDefUpgrade);
            cancelButton.AddListener(domain.HandleOnCanceledUpgrade);
            closeButton.AddListener(domain.HandleOnRequestClose);
        }

        public void Show(CardData cardData)
        {
            card.SetCardData(cardData);
            upgradeButtonGroup.Show();
            cancelButton.CanvasGroup.Hide();
            closeButton.CanvasGroup.Hide();

            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;

            cancelButton.CanvasGroup.interactable = true;
            cancelButton.CanvasGroup.blocksRaycasts = true;

            forgeVisualEffect.PlayShowEffect(CanvasGroup, RectTransform, cancelButton.CanvasGroup, _originAnchoredPosition.x);
        }

        public void Hide()
        {
            forgeVisualEffect.PlayHideEffect(CanvasGroup, RectTransform, cancelButton.CanvasGroup, closeButton.CanvasGroup, _originAnchoredPosition.x);
        }

        public void UpgradeAtkRate(Action callback = null)
        {
            DisableUpgradeInteraction();
            forgeVisualEffect.PlayAtkUpgradeEffect(Card, cancelButton.CanvasGroup, closeButton.CanvasGroup, callback);
        }

        public void UpgradeBothRate(Action callback = null)
        {
            DisableUpgradeInteraction();
            forgeVisualEffect.PlayBothUpgradeEffect(Card, cancelButton.CanvasGroup, closeButton.CanvasGroup, callback);
        }

        public void UpgradeDefRate(Action callback = null)
        {
            DisableUpgradeInteraction();
            forgeVisualEffect.PlayDefUpgradeEffect(Card, cancelButton.CanvasGroup, closeButton.CanvasGroup, callback);
        }

        private void DisableUpgradeInteraction()
        {
            upgradeButtonGroup.interactable = false;
            upgradeButtonGroup.blocksRaycasts = false;

            cancelButton.CanvasGroup.interactable = false;
            cancelButton.CanvasGroup.blocksRaycasts = false;
        }
    }
}
