using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Jongmin
{
    public class CardInvenSystem : MonoBehaviour
    {
        private CardInvenView _view;
        private CardContainer _container;
        private InventoryCardFactory _factory;

        public event Action RequestCloseView;

        public void Construct(CardInvenView view, CardContainer container, InventoryCardFactory factory)
        {
            _view = view;
            _container = container;
            _factory = factory;
        }
        
        public void OpenView()
        {
            _view.Show();
            RefreshCardInventory();
        }

        public void CloseView()
        {
            _view.Hide();
            ClearCardInventory();
            RequestCloseView?.Invoke();
        }

        public void RefreshCardInventory()
        {
            ClearCardInventory();
            SetCardInventory();
        }

        private void SetCardInventory()
        {
            var displayCards = GetUserCards();

            foreach (var kvp in displayCards)
            {
                var cardId = kvp.Key;
                var cardData = kvp.Value;

                if (cardData == null)
                {
                    continue;
                }

                var card = _factory.Create();
                card.SetCardData(cardData);
                _container.Add(card);
            }
        }

        private Dictionary<string, CardData> GetUserCards()
        {
            var cards = new Dictionary<string, CardData>();

            if (DataCenter.Instance.userDeck == null)
            {
                return cards;
            }

            foreach (var data in DataCenter.Instance.userDeck)
            {
                cards[data.id] = data;
            }

            return cards;
        }

        private void ClearCardInventory()
        {
            var tempCards = new List<Card>(_container.Cards);
            foreach (var card in tempCards)
            {
                _container.Remove(card);
                _factory.Release(card);
            }
        }
    }
}