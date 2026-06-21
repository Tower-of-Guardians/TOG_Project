using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Jongmin
{
    public class EffectSystem : MonoBehaviour
    {
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

        public IEnumerator DiscardHandCards(IReadOnlyList<Card> handCards, HandSystem handSystem, Vector3 destination)
        {
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

        private IEnumerator DrawHandSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            card.transform.DOKill();

            var sequence = DOTween.Sequence();
            sequence.Join(card.RectTransform.DOJump(destination, 0f, 1, duration));
            sequence.Join(card.RectTransform.DOScale(0.4f * Vector3.one, duration));
            sequence.OnComplete(() => completeAction());

            yield return sequence.WaitForCompletion();
            RemoveCard(card);
        }

        private IEnumerator DiscardHandSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            card.transform.DOKill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(card.RectTransform.DOJump(destination, 50f, 1, duration));
            sequence.Join(card.RectTransform.DOScale(0.11f * Vector3.one, duration));
            sequence.Join(card.RectTransform.DORotate(-180f * Vector3.forward, duration, RotateMode.LocalAxisAdd));
            sequence.Join(card.View.CanvasGroup.DOFade(0.5f, duration));
            sequence.OnComplete(() => completeAction());
            
            yield return sequence.WaitForCompletion();
            RemoveCard(card);
        }

        private IEnumerator RevertHandSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            card.transform.DOKill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(card.RectTransform.DOJump(destination, 0f, 1, duration).SetEase(Ease.InQuad));
            sequence.Join(card.RectTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));
            sequence.OnComplete(() => completeAction());
            
            yield return sequence.WaitForCompletion();
            RemoveCard(card);
        }
        
        private IEnumerator DiscardDiscardSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            card.transform.DOKill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(card.RectTransform.DOJump(destination, -50f, 1, duration).SetEase(Ease.InQuad));
            sequence.Join(card.RectTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));
            sequence.Join(card.View.CanvasGroup.DOFade(0.5f, duration));
            sequence.OnComplete(() => completeAction());
            
            yield return sequence.WaitForCompletion();
            RemoveCard(card);
        }

        private IEnumerator DiscardFieldSubRoutine(Card card, Vector3 destination, float duration, Action completeAction)
        {
            card.transform.DOKill();
            
            var sequence = DOTween.Sequence();
            sequence.Join(card.RectTransform.DOJump(destination, 150f, 1, duration).SetEase(Ease.InOutQuad));
            sequence.Join(card.RectTransform.DOScale(0.11f * Vector3.one, duration).SetEase(Ease.InQuad));
            sequence.Join(card.RectTransform.DORotate(-180f * Vector3.forward, duration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));
            sequence.Join(card.View.CanvasGroup.DOFade(0.5f, duration));
            sequence.OnComplete(() => completeAction());
            
            yield return sequence.WaitForCompletion();
            RemoveCard(card);
        }
    }
}