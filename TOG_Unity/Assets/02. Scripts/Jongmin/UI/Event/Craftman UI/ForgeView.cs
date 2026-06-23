using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class ForgeView : ViewBase
    {
        [SerializeField] private Card card;
        [SerializeField] private CanvasGroup upgradeButtonGroup;
        [SerializeField] private ButtonView atkUpgradeButton;
        [SerializeField] private ButtonView bothUpgradeButton;
        [SerializeField] private ButtonView defUpgradeButton;
        [SerializeField] private ButtonView cancelButton;
        [SerializeField] private ButtonView closeButton;
        
        private Vector2 _originAnchoredPosition;
        private Tween _toggleTween;
        private Tween _upgradeTween;

        public Card Card => card;
        
        private void Awake()
        {
            _originAnchoredPosition = RectTransform.anchoredPosition;
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
            
            _toggleTween?.Kill();

            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            
            cancelButton.CanvasGroup.interactable = true;
            cancelButton.CanvasGroup.blocksRaycasts = true;
            
            var sequence = DOTween.Sequence();
            sequence.Join(CanvasGroup.DOFade(1f, 0.5f));
            sequence.Join(RectTransform.DOAnchorPosX(_originAnchoredPosition.x + 960f, 0.5f));
            sequence.Append(cancelButton.CanvasGroup.DOFade(1f, 0.5f));

            _toggleTween = sequence;
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(closeButton.CanvasGroup.DOFade(0f, 0.5f).OnComplete(closeButton.CanvasGroup.Hide));
            sequence.Join(cancelButton.CanvasGroup.DOFade(0f, 0.5f).OnComplete(cancelButton.CanvasGroup.Hide));
            sequence.Append(CanvasGroup.DOFade(0f, 0.5f));
            sequence.Join(RectTransform.DOAnchorPosX(_originAnchoredPosition.x, 0.5f));
            sequence.OnComplete(CanvasGroup.Hide);

            _toggleTween = sequence;
        }

        public void UpgradeAtkRate(TweenCallback callback = null)
        {
            upgradeButtonGroup.interactable = false;
            upgradeButtonGroup.blocksRaycasts = false;
            
            cancelButton.CanvasGroup.interactable = false;
            cancelButton.CanvasGroup.blocksRaycasts = false;
            
            _upgradeTween?.Kill();
            
            var originCardAnchoredPosition = Card.RectTransform.anchoredPosition;
            Card.RectTransform.anchoredPosition = originCardAnchoredPosition + new Vector2(0f, 100f);
            Card.RectTransform.localScale = 2.2f * Vector3.one;
            
            var sequence = DOTween.Sequence();
            sequence.Join(cancelButton.CanvasGroup.DOFade(0f, 0.5f));
            sequence.Join(Card.RectTransform.DOAnchorPosY(originCardAnchoredPosition.y, 0.5f));
            sequence.Join(Card.RectTransform.DOScale(2f, 0.5f));
            sequence.AppendCallback(() =>
            {
                Card.View.AtkLabel.Label.text = $"{Card.CardData.ATK}";
                Card.View.AtkLabel.RectTransform.localScale = 2f * Vector3.one;
            });
            sequence.Join(Card.View.AtkLabel.RectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            sequence.Append(closeButton.CanvasGroup.DOFade(1f, 0.5f));
            sequence.OnComplete(() =>
            {
                closeButton.CanvasGroup.Show();
                callback?.Invoke();
            });

            _upgradeTween = sequence;
        }

        public void UpgradeBothRate(TweenCallback callback = null)
        {
            upgradeButtonGroup.interactable = false;
            upgradeButtonGroup.blocksRaycasts = false;
            
            cancelButton.CanvasGroup.interactable = false;
            cancelButton.CanvasGroup.blocksRaycasts = false;
            
            _upgradeTween?.Kill();
            
            var originCardAnchoredPosition = Card.RectTransform.anchoredPosition;
            Card.RectTransform.anchoredPosition = originCardAnchoredPosition + new Vector2(0f, 100f);
            Card.RectTransform.localScale = 2.2f * Vector3.one;
            
            var sequence = DOTween.Sequence();
            sequence.Join(cancelButton.CanvasGroup.DOFade(0f, 0.5f));
            sequence.Join(Card.RectTransform.DOAnchorPosY(originCardAnchoredPosition.y, 0.5f));
            sequence.Join(Card.RectTransform.DOScale(2f, 0.5f));
            sequence.AppendCallback(() =>
            {
                Card.View.AtkLabel.Label.text = $"{Card.CardData.ATK}";
                Card.View.AtkLabel.RectTransform.localScale = 2f * Vector3.one;
            });
            sequence.Join(Card.View.AtkLabel.RectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            sequence.AppendCallback(() =>
            {
                Card.View.DefLabel.Label.text = $"{Card.CardData.DEF}";
                Card.View.DefLabel.RectTransform.localScale = 2f * Vector3.one;
            });
            sequence.Join(Card.View.DefLabel.RectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            sequence.Append(closeButton.CanvasGroup.DOFade(1f, 0.5f));
            sequence.OnComplete(() =>
            {
                closeButton.CanvasGroup.Show();
                callback?.Invoke();
            });

            _upgradeTween = sequence;
        }

        public void UpgradeDefRate(TweenCallback callback = null)
        {
            upgradeButtonGroup.interactable = false;
            upgradeButtonGroup.blocksRaycasts = false;
            
            cancelButton.CanvasGroup.interactable = false;
            cancelButton.CanvasGroup.blocksRaycasts = false;
            
            _upgradeTween?.Kill();
            
            var originCardAnchoredPosition = Card.RectTransform.anchoredPosition;
            Card.RectTransform.anchoredPosition = originCardAnchoredPosition + new Vector2(0f, 100f);
            Card.RectTransform.localScale = 2.2f * Vector3.one;
            
            var sequence = DOTween.Sequence();
            sequence.Join(cancelButton.CanvasGroup.DOFade(0f, 0.5f));
            sequence.Join(Card.RectTransform.DOAnchorPosY(originCardAnchoredPosition.y, 0.5f));
            sequence.Join(Card.RectTransform.DOScale(2f, 0.5f));
            sequence.AppendCallback(() =>
            {
                Card.View.DefLabel.Label.text = $"{Card.CardData.DEF}";
                Card.View.DefLabel.RectTransform.localScale = 2f * Vector3.one;
            });
            sequence.Join(Card.View.DefLabel.RectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            sequence.Append(closeButton.CanvasGroup.DOFade(1f, 0.5f));
            sequence.OnComplete(() =>
            {
                closeButton.CanvasGroup.Show();
                callback?.Invoke();
            });

            _upgradeTween = sequence;
        }
    }
}