using System.Collections.Generic;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class CardContainer
    {
        private readonly List<Card> _cards = new();
        
        public IReadOnlyList<Card> Cards => _cards;
        public int Count => _cards.Count;

        public void Add(Card card)
        {
            _cards.Add(card);
        }

        public void Remove(Card card)
        {
            _cards.Remove(card);
        }

        public void Clear()
        {
            _cards.Clear();
        }

        public bool IsExist(Card card)
        {
            return _cards.Contains(card);
        }

        public Card Get(int index)
        {
            return _cards[index];
        }

        public void Insert(Card from, Card to)
        {
            if (!TryGetIndex(from, out var fromIndex) || !TryGetIndex(to, out var toIndex))
            {
                return;
            }

            if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex)
            {
                return;
            }
            
            _cards.RemoveAt(fromIndex);
            _cards.Insert(toIndex, from);
        }

        public bool IsPriority(Card lhs, Card rhs)
        {
            if (!TryGetIndex(lhs, out var fromIndex) || !TryGetIndex(rhs, out var toIndex))
            {
                return false;
            }

            return fromIndex < toIndex;
        }

        public bool TryGetIndex(Card card, out int index)
        {
            if (!IsExist(card))
            {
                index = -1;
                return false;
            }
            
            index = _cards.IndexOf(card);
            return true;
        }
    }
}