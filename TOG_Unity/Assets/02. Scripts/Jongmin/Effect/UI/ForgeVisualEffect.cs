using System;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    [ManagedEffect("UI", "Forge", 101)]
    public class ForgeVisualEffect : MonoBehaviour
    {
        [BigHeader("Show / Hide")]
        [Header("Show")]
        [SerializeField] private float showDuration = 0.5f;
        [SerializeField] private float showMoveOffsetX = 960f;
        [SerializeField] private float cancelButtonFadeInDuration = 0.5f;

        [Header("Hide")]
        [SerializeField] private float hideButtonFadeOutDuration = 0.5f;
        [SerializeField] private float hideDuration = 0.5f;

        [Space(30f)]
        [BigHeader("Upgrade")]
        [Header("Card")]
        [SerializeField] private float upgradeCardStartOffsetY = 100f;
        [SerializeField] private float upgradeCardStartScale = 2.2f;
        [SerializeField] private float upgradeCardEndScale = 2f;
        [SerializeField] private float upgradeCardDuration = 0.5f;

        [Header("Label")]
        [SerializeField] private float upgradeLabelStartScale = 2f;
        [SerializeField] private float upgradeLabelEndScale = 1f;
        [SerializeField] private float upgradeLabelDuration = 0.5f;
        [SerializeField] private Ease upgradeLabelEase = Ease.OutBack;

        [Header("Close Button")]
        [SerializeField] private float closeButtonFadeInDuration = 0.5f;

        private Tween _toggleTween;
        private Tween _upgradeTween;

        public Tween PlayShowEffect(CanvasGroup viewGroup,
                                    RectTransform viewRectTransform,
                                    CanvasGroup cancelButtonGroup,
                                    float originAnchoredPositionX)
        {
            _toggleTween?.Kill();

            var sequence = DOTween.Sequence();
            sequence.Join(viewGroup.DOFade(1f, showDuration));
            sequence.Join(viewRectTransform.DOAnchorPosX(originAnchoredPositionX + showMoveOffsetX, showDuration));
            sequence.Append(cancelButtonGroup.DOFade(1f, cancelButtonFadeInDuration));

            _toggleTween = sequence;
            return _toggleTween;
        }

        public Tween PlayHideEffect(CanvasGroup viewGroup,
                                    RectTransform viewRectTransform,
                                    CanvasGroup cancelButtonGroup,
                                    CanvasGroup closeButtonGroup,
                                    float originAnchoredPositionX)
        {
            _toggleTween?.Kill();

            var sequence = DOTween.Sequence();
            sequence.Join(closeButtonGroup.DOFade(0f, hideButtonFadeOutDuration).OnComplete(closeButtonGroup.Hide));
            sequence.Join(cancelButtonGroup.DOFade(0f, hideButtonFadeOutDuration).OnComplete(cancelButtonGroup.Hide));
            sequence.Append(viewGroup.DOFade(0f, hideDuration));
            sequence.Join(viewRectTransform.DOAnchorPosX(originAnchoredPositionX, hideDuration));
            sequence.OnComplete(viewGroup.Hide);

            _toggleTween = sequence;
            return _toggleTween;
        }

        public Tween PlayAtkUpgradeEffect(Card card,
                                          CanvasGroup cancelButtonGroup,
                                          CanvasGroup closeButtonGroup,
                                          Action callback = null)
        {
            _upgradeTween?.Kill();

            var originCardAnchoredPosition = SetUpgradeCardStartState(card);
            var sequence = CreateUpgradeIntroSequence(card, cancelButtonGroup, originCardAnchoredPosition);
            sequence.AppendCallback(() => SetAtkLabel(card));
            sequence.Join(PlayAtkLabelScale(card));
            sequence.Append(CreateCloseButtonFadeIn(closeButtonGroup, callback));

            _upgradeTween = sequence;
            return _upgradeTween;
        }

        public Tween PlayBothUpgradeEffect(Card card,
                                           CanvasGroup cancelButtonGroup,
                                           CanvasGroup closeButtonGroup,
                                           Action callback = null)
        {
            _upgradeTween?.Kill();

            var originCardAnchoredPosition = SetUpgradeCardStartState(card);
            var sequence = CreateUpgradeIntroSequence(card, cancelButtonGroup, originCardAnchoredPosition);
            sequence.AppendCallback(() => SetAtkLabel(card));
            sequence.Join(PlayAtkLabelScale(card));
            sequence.AppendCallback(() => SetDefLabel(card));
            sequence.Join(PlayDefLabelScale(card));
            sequence.Append(CreateCloseButtonFadeIn(closeButtonGroup, callback));

            _upgradeTween = sequence;
            return _upgradeTween;
        }

        public Tween PlayDefUpgradeEffect(Card card,
                                          CanvasGroup cancelButtonGroup,
                                          CanvasGroup closeButtonGroup,
                                          Action callback = null)
        {
            _upgradeTween?.Kill();

            var originCardAnchoredPosition = SetUpgradeCardStartState(card);
            var sequence = CreateUpgradeIntroSequence(card, cancelButtonGroup, originCardAnchoredPosition);
            sequence.AppendCallback(() => SetDefLabel(card));
            sequence.Join(PlayDefLabelScale(card));
            sequence.Append(CreateCloseButtonFadeIn(closeButtonGroup, callback));

            _upgradeTween = sequence;
            return _upgradeTween;
        }

        private Vector2 SetUpgradeCardStartState(Card card)
        {
            var originCardAnchoredPosition = card.RectTransform.anchoredPosition;
            card.RectTransform.anchoredPosition = originCardAnchoredPosition + new Vector2(0f, upgradeCardStartOffsetY);
            card.RectTransform.localScale = upgradeCardStartScale * Vector3.one;

            return originCardAnchoredPosition;
        }

        private Sequence CreateUpgradeIntroSequence(Card card,
                                                    CanvasGroup cancelButtonGroup,
                                                    Vector2 originCardAnchoredPosition)
        {
            var sequence = DOTween.Sequence();
            sequence.Join(cancelButtonGroup.DOFade(0f, upgradeCardDuration));
            sequence.Join(card.RectTransform.DOAnchorPosY(originCardAnchoredPosition.y, upgradeCardDuration));
            sequence.Join(card.RectTransform.DOScale(upgradeCardEndScale, upgradeCardDuration));

            return sequence;
        }

        private void SetAtkLabel(Card card)
        {
            card.View.AtkLabel.Label.text = $"{card.CardData.ATK}";
            card.View.AtkLabel.RectTransform.localScale = upgradeLabelStartScale * Vector3.one;
        }

        private void SetDefLabel(Card card)
        {
            card.View.DefLabel.Label.text = $"{card.CardData.DEF}";
            card.View.DefLabel.RectTransform.localScale = upgradeLabelStartScale * Vector3.one;
        }

        private Tween PlayAtkLabelScale(Card card)
            => card.View.AtkLabel.RectTransform
                .DOScale(upgradeLabelEndScale, upgradeLabelDuration)
                .SetEase(upgradeLabelEase);

        private Tween PlayDefLabelScale(Card card)
            => card.View.DefLabel.RectTransform
                .DOScale(upgradeLabelEndScale, upgradeLabelDuration)
                .SetEase(upgradeLabelEase);

        private Tween CreateCloseButtonFadeIn(CanvasGroup closeButtonGroup, Action callback)
        {
            return closeButtonGroup
                .DOFade(1f, closeButtonFadeInDuration)
                .OnComplete(() =>
                {
                    closeButtonGroup.Show();
                    callback?.Invoke();
                });
        }
    }
}
