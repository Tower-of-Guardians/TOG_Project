using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class EffectSystem : MonoBehaviour
    {
        [BigHeader("Card Move Effect")]
        [Header("Draw To Hand")]
        [SerializeField] private float drawMoveArcHeight = 180f;
        [SerializeField] private float drawMoveEndScale = 0.4f;
        [SerializeField] private Ease drawMoveEase = Ease.InOutQuad;

        [Header("Hand / Field To Throw")]
        [SerializeField] private float discardMoveArcHeight = 180f;
        [SerializeField] private float discardMoveEndScale = 0.11f;
        [SerializeField] private float discardMoveFadeAlpha = 0.5f;
        [SerializeField] private Ease discardMoveEase = Ease.InOutQuad;

        private CardContainer _container;
        private EffectCardFactory _factory;

        public void Construct(CardContainer container, EffectCardFactory factory)
        {
            _container = container;
            _factory = factory;
        }

        public Card CreateCard(BattleCardData battleCardData)
        {
            var card = _factory.Create();
            card.SetBattleCardData(battleCardData, CardType.Effect);
            _container.Add(card);

            return card;
        }
        
        public void RemoveCard(Card card)
        {
            _container.Remove(card);
            ResetCardState(card);
            _factory.Release(card);
        }

        public IEnumerator DrawHandCards(IReadOnlyList<BattleCardData> battleCardDatas, HandSystem handSystem, Vector3 source, Vector3 destination)
        {
            var currentCount = 0;
            var completeCount = battleCardDatas.Count;
            
            foreach (var battleCardData in battleCardDatas)
            {
                var effectCard = CreateCard(battleCardData);
                effectCard.transform.position = source;
                effectCard.RectTransform.localScale = 0.2f * Vector3.one;

                StartCoroutine(DrawHandSubRoutine(effectCard, destination, 0.25f, () =>
                {
                    currentCount++;
                    handSystem.CreateCard(effectCard.BattleCardData);
                }));
                
                yield return new WaitForSeconds(0.075f);
            }
            
            yield return new WaitUntil(() => currentCount >= completeCount);
        }

        public IEnumerator DiscardHandCards(IReadOnlyList<Card> handCards, HandSystem handSystem, ImageView battleView, Vector3 destination)
        {
            yield return battleView.CanvasGroup.DOFade(0.7f, 0.3f).OnComplete(() =>
            {
                battleView.CanvasGroup.interactable = true;
                battleView.CanvasGroup.blocksRaycasts = true;
            }).WaitForCompletion();
            
            var currentCount = 0;
            var completeCount = handCards.Count;
            
            var cards = new List<Card>(handCards);
            
            foreach (var handCard in cards)
            {
                var battleCardData = handCard.BattleCardData;
                
                var position = handCard.transform.position;
                var rotation = handCard.RectTransform.rotation;
                var localScale = 0.66f * Vector3.one;

                handSystem.RemoveCard(handCard);
                
                var effectCard = CreateCard(battleCardData);

                effectCard.RectTransform.position = position;
                effectCard.RectTransform.rotation = rotation;
                effectCard.RectTransform.localScale = localScale;

                StartCoroutine(DiscardHandSubRoutine(effectCard, destination, 0.5f, () => currentCount++));

                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitUntil(() => currentCount >= completeCount);
        }

        public void RevertDiscardCards(IReadOnlyList<Card> discardCards, HandSystem handSystem, DiscardSystem discardSystem, Vector3 destination)
        {
            var cards = new List<Card>(discardCards);

            foreach (var discardCard in cards)
            {
                if (discardCard == null)
                {
                    continue;
                }

                var battleCardData = discardCard.BattleCardData;

                var startPosition = discardCard.transform.position;
                var startRotation = discardCard.RectTransform.rotation;
                var startScale = 0.66f * Vector3.one;

                discardSystem.RemoveCard(discardCard);

                var effectCard = CreateCard(battleCardData);

                effectCard.transform.position = startPosition;
                effectCard.RectTransform.rotation = startRotation;
                effectCard.RectTransform.localScale = startScale;

                StartCoroutine(RevertHandSubRoutine(effectCard, destination, 0.35f, () => handSystem.CreateCard(battleCardData)));
            }
        }

        public void DiscardDiscardCards(IReadOnlyList<Card> discardCards, DiscardSystem discardSystem, Vector3 destination)
        {
            var cards = new List<Card>(discardCards);

            foreach (var discardCard in cards)
            {
                if (discardCard == null)
                {
                    continue;
                }
                
                var battleCardData = discardCard.BattleCardData;

                var startPosition = discardCard.transform.position;
                var startRotation = discardCard.RectTransform.rotation;
                var startScale = 0.44f * Vector3.one;

                discardSystem.RemoveCard(discardCard);

                var effectCard = CreateCard(battleCardData);

                effectCard.transform.position = startPosition;
                effectCard.RectTransform.rotation = startRotation;
                effectCard.RectTransform.localScale = startScale;

                StartCoroutine(DiscardDiscardSubRoutine(effectCard, destination, 0.5f, () =>
                {
                    GameData.Instance.UseCard(effectCard.CardData.id);
                    GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
                }));
            }
        }

        public IEnumerator DiscardFieldCards(IReadOnlyList<Card> fieldCards, FieldSystem fieldSystem, FieldView fieldView, Vector3 destination)
        {
            yield return fieldView.ToggleViewActive(false).WaitForCompletion();
            
            var currentCount = 0;
            var completeCount = fieldCards.Count;
            
            var cards = new List<Card>(fieldCards);
            
            foreach (var fieldCard in cards)
            {
                var battleCardData = fieldCard.BattleCardData;
                
                var position = fieldCard.transform.position;
                var rotation = fieldCard.RectTransform.rotation;
                var localScale = 0.66f * Vector3.one;

                fieldSystem.RemoveCard(fieldCard);
                
                var effectCard = CreateCard(battleCardData);

                effectCard.RectTransform.position = position;
                effectCard.RectTransform.rotation = rotation;
                effectCard.RectTransform.localScale = localScale;

                StartCoroutine(DiscardFieldSubRoutine(effectCard, destination, 0.5f, () => currentCount++));

                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitUntil(() => currentCount >= completeCount);
        }

        public void EnableBattleView(ImageView battleView, FieldView atkFieldView, FieldView defFieldView)
        {
            battleView.CanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                battleView.CanvasGroup.interactable = false;
                battleView.CanvasGroup.blocksRaycasts = false;
            });
            
            atkFieldView.ToggleViewActive(true);
            defFieldView.ToggleViewActive(true);
        }

        private IEnumerator DrawHandSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            yield return PlayCardArcMoveEffect(
                card,
                destination,
                duration,
                drawMoveArcHeight,
                drawMoveEndScale * Vector3.one,
                drawMoveEase
            );

            completeAction?.Invoke();
            RemoveCard(card);
        }

        private IEnumerator DiscardHandSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            yield return PlayDiscardMoveEffect(card, destination, duration);

            GameData.Instance.UseCard(card.CardData.id);
            GameData.Instance.handDeck.Remove(card.CardData.id);
            GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
            RemoveCard(card);
            completeAction?.Invoke();
        }

        private IEnumerator RevertHandSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            KillCardTweens(card);
            
            var sequence = DOTween.Sequence();
            sequence.Join(card.RectTransform.DOJump(destination, 0f, 1, duration).SetEase(Ease.InQuad));
            sequence.Join(card.RectTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));
            sequence.OnComplete(() => completeAction());
            
            yield return sequence.WaitForCompletion();
            RemoveCard(card);
        }
        
        private IEnumerator DiscardDiscardSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            yield return PlayDiscardMoveEffect(card, destination, duration);

            completeAction?.Invoke();
            RemoveCard(card);
        }

        private IEnumerator DiscardFieldSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            yield return PlayDiscardMoveEffect(card, destination, duration);

            GameData.Instance.UseCard(card.CardData.id);
            GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
            RemoveCard(card);
            completeAction?.Invoke();
        }

        private IEnumerator PlayDiscardMoveEffect(Card card, Vector3 destination, float duration)
        {
            yield return PlayCardArcMoveEffect(
                card,
                destination,
                duration,
                discardMoveArcHeight,
                discardMoveEndScale * Vector3.one,
                discardMoveEase,
                discardMoveFadeAlpha
            );
        }

        private IEnumerator PlayCardArcMoveEffect(Card card,
                                                  Vector3 destination,
                                                  float duration,
                                                  float arcHeight,
                                                  Vector3 endScale,
                                                  Ease moveEase,
                                                  float fadeAlpha = -1f)
        {
            KillCardTweens(card);

            var rectTransform = card.RectTransform;
            var startPosition = rectTransform.position;
            var controlPosition = startPosition + Vector3.up * arcHeight;

            var sequence = DOTween.Sequence();
            sequence.Join(DOTween.To(
                () => 0f,
                progress =>
                {
                    var position = EvaluateQuadraticBezier(startPosition, controlPosition, destination, progress);
                    var tangent = EvaluateQuadraticBezierTangent(startPosition, controlPosition, destination, progress);

                    rectTransform.position = position;
                    RotateCardHeadToDirection(rectTransform, tangent);
                },
                1f,
                duration
            ).SetEase(moveEase));
            sequence.Join(rectTransform.DOScale(endScale, duration).SetEase(Ease.InQuad));
            
            if (fadeAlpha >= 0f)
            {
                sequence.Join(card.View.CanvasGroup.DOFade(fadeAlpha, duration));
            }

            yield return sequence.WaitForCompletion();
        }

        private Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float progress)
        {
            var inverseProgress = 1f - progress;
            return inverseProgress * inverseProgress * start
                   + 2f * inverseProgress * progress * control
                   + progress * progress * end;
        }

        private Vector3 EvaluateQuadraticBezierTangent(Vector3 start, Vector3 control, Vector3 end, float progress)
        {
            return 2f * (1f - progress) * (control - start)
                   + 2f * progress * (end - control);
        }

        private void RotateCardHeadToDirection(RectTransform rectTransform, Vector3 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void KillCardTweens(Card card)
        {
            card.transform.DOKill();
            card.RectTransform.DOKill();
            card.View.CanvasGroup.DOKill();
        }

        private void ResetCardState(Card card)
        {
            KillCardTweens(card);
            card.RectTransform.localRotation = Quaternion.identity;
            card.View.CanvasGroup.alpha = 1f;
        }
    }
}
