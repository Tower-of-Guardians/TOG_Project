using System;
using UnityEngine;

namespace Jongmin
{
    public delegate void DiscardCancelEffect(DiscardSystem discardSystem, CardContainer cardContainer);
    public delegate void DiscardEffect(DiscardSystem discardSystem, CardContainer cardContainer);
    
    public class DiscardSystem : MonoBehaviour, ICardDropTarget<DiscardDomain>
    {
        private DiscardView _view;
        private CardContainer _container;
        private DiscardCardLayout _layout;
        private DiscardCardFactory _factory;
        
        public event Action<int> RequestUpdateThrowCount; 
        public event Action<bool> RequestUpdateThrowAction;
        public event Action<bool> OnDiscardViewVisibilityChanged;
        
        public DiscardCancelEffect DiscardCancelEffect;
        public DiscardEffect DiscardEffect;
        
        public Card HoverCard { get; set; }

        public void Construct(DiscardView view, CardContainer container, DiscardCardLayout layout, DiscardCardFactory factory)
        {
            _view = view;
            _container = container;
            _layout = layout;
            _factory = factory;
        }

        public void CreateCard(BattleCardData battleCardData)
        {
            var card = _factory.Create();
            card.SetBattleCardData(battleCardData, CardType.Discard);
            _container.Add(card);
            _layout.UpdateLayout(PreviewLayoutMode.None, isAnime: false);
            
            RequestUpdateThrowCount?.Invoke(1);
        }

        public void RemoveCard(Card card, bool unused = true)
        {
            _container.Remove(card);
            _factory.Release(card);
            _layout.UpdateLayout(PreviewLayoutMode.None, isAnime: true);
            
            RequestUpdateThrowCount?.Invoke(-1);
        }

        public void OpenView()
        {
            _view.Show();
            OnDiscardViewVisibilityChanged?.Invoke(true);
        }

        public void CloseView()
        {
            _view.Hide();
            DiscardCancelEffect?.Invoke(this, _container);
            OnDiscardViewVisibilityChanged?.Invoke(false);
        }

        public void DiscardCards()
        {
            _view.Hide();
            DiscardEffect?.Invoke(this, _container);
            RequestUpdateThrowAction?.Invoke(false);
            OnDiscardViewVisibilityChanged?.Invoke(false);
        }

        public void UpdateOpenButtonState(bool isActive)
        {
            _view.UpdateOpenButtonState(isActive);
        }

        public void UpdateDiscardCount(ActionData data, bool canThrow)
        {
            _view.UpdateDiscardButtonState(data.Current > 0);
        }
    }
}