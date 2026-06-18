using System;
using UnityEngine;

namespace Jongmin
{
    public class HandSystem : MonoBehaviour, ICardDropTarget<HandDomain>
    {
        private CardContainer _container;
        private HandCardLayout _layout;
        private HandCardFactory _factory;
        
        public event Action<bool> OnTogglePreviews;
        
        public Card HoverCard { get; set; }

        public void Construct(CardContainer container, HandCardLayout layout, HandCardFactory factory)
        {
            _container = container;
            _layout = layout;
            _factory = factory;
        }

        public void CreateCard(BattleCardData battleCardData)
        {
            var card = _factory.Create();
            card.SetBattleCardData(battleCardData, CardType.Hand);
            _container.Add(card);
            _layout.UpdateLayout();
        }

        public void RemoveCard(Card card, bool isUpdateLayout = true)
        {
            _container.Remove(card);
            _factory.Release(card);

            if (isUpdateLayout)
            {
                _layout.UpdateLayout();
            }
        }
        
        public void ToggleFieldPreview(bool isActive)
        {
            OnTogglePreviews?.Invoke(isActive);
        }
    }
}