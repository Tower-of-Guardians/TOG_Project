using System.Collections.Generic;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class SeriesView : ViewBase
    {
        [SerializeField] private Transform cardGroup;

        private Card[] _cards;
        private Tween _toggleTween;

        private void Awake()
        {
            _cards = cardGroup.GetComponentsInChildren<Card>();
        }

        public void Show()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(1f, 0.25f).OnComplete(CanvasGroup.Show);
        }

        public void Hide()
        {
            _toggleTween?.Kill();
            _toggleTween = CanvasGroup.DOFade(0f, 0.25f).OnComplete(CanvasGroup.Hide);
        }

        public void SetSeries(IReadOnlyList<CardData> cardDatas)
        {
            foreach (var card in _cards)
            {
                card.gameObject.SetActive(false);
            }

            for (var i = 0; i < cardDatas.Count; i++)
            {
                var card = _cards[i];
                card.gameObject.SetActive(true);
                card.SetCardData(cardDatas[i]);
            }
        }
    }
}