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

        private void Awake()
        {
            _cards = cardGroup.GetComponentsInChildren<Card>();
        }

        public void Show()
        {
            CanvasGroup.Show();
        }

        public void Hide()
        {
            CanvasGroup.Hide();
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