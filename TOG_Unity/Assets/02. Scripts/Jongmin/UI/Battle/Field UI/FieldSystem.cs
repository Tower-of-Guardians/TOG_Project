using System;
using UnityEngine;

namespace Jongmin
{
    public abstract class FieldSystem : MonoBehaviour
    {
        private FieldView _view;
        protected CardContainer Container;
        protected FieldCardLayout Layout;
        protected FieldCardFactory Factory;

        private const int MaxCardCount = 4;

        public event Action<int> RequestUpdateActionCount;
        
        public Card HoverCard { get; set; }
        public bool CanInteraction { get; private set; } = true;
        protected bool CanAdd => Container.Count < MaxCardCount;
        public virtual FieldType FieldType { get; protected set; }

        public void Construct(FieldView view, CardContainer container, FieldCardLayout layout, FieldCardFactory factory)
        {
            _view = view;
            Container = container;
            Layout = layout;
            Factory = factory;
        }
        
        public bool IsExist(Card card)
        {
            return Container.IsExist(card);
        }

        public abstract void CreateCard(BattleCardData battleCardData);

        public void RequestUpdateActionCountEvent(int count)
        {
            RequestUpdateActionCount?.Invoke(count);
        }

        public void RemoveCard(Card card, bool unused = true)
        {
            Container.Remove(card);
            Factory.Release(card);
            RequestUpdateActionCount?.Invoke(-1);
        }

        public void TogglePreview(bool isActive)
        {
            if (!CanInteraction)
            {
                return;
            }

            switch (isActive)
            {
                case true when CanAdd:
                    _view.TogglePreview(true);
                    Layout.UpdateLayout(FieldPreviewMode.Insert);
                    break;
                
                case false:
                    _view.TogglePreview(false);
                    Layout.UpdateLayout(FieldPreviewMode.None);
                    break;
            }
        }

        public void UpdateInteraction(bool isActive)
        {
            CanInteraction = !isActive;
        }
    }
}