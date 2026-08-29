using System.Collections.Generic;
using UnityEngine;

namespace Jongmin
{
    public abstract class CompactInvenSystem : MonoBehaviour
    {
        protected CompactInvenView View;
        protected CompactInvenSlotFactory Factory;
        protected List<CompactInvenSlot> Container = new();

        public void Construct(CompactInvenView view, CompactInvenSlotFactory factory)
        {
            View = view;
            Factory = factory;
        }
        
        public void OpenView()
        {
            RefreshView();
            View.Show();
        }

        public void RefreshView()
        {
            RemoveAllSlots();
            DataCenter.Instance.SortUserCards(SortType.Grade, true);

            foreach (var cardData in DataCenter.Instance.userDeck)
            {
                CreateSlot(cardData);
            }
        }

        public void CloseView()
        {
            View.Hide(RemoveAllSlots);
        }
        
        protected abstract void CreateSlot(CardData cardData);
        
        private void RemoveSlot(CompactInvenSlot slot)
        {
            Container.Remove(slot);
            Factory.Release(slot);
        }

        private void RemoveAllSlots()
        {
            var tempContainer = new List<CompactInvenSlot>(Container);

            foreach (var slot in tempContainer)
            {
                RemoveSlot(slot);
            }
            
            Container.Clear();
        }
    }
}
