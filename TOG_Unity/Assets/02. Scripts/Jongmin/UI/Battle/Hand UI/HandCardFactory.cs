using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class HandCardFactory
    {
        private readonly HandView _view;
        private readonly HandEventSystem _eventSystem;
        private readonly Card _prefab;
    
        public HandCardFactory(HandView view, HandEventSystem eventSystem)
        {
            _view = view;
            _eventSystem = eventSystem;
            _prefab = PrefabManager.CachePrefab<Card>();
        }

        public Card Create()
        {
            var cardObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            cardObject.transform.SetParent(_view.CardRoot, false);
        
            var card = cardObject.GetComponent<Card>();
            card.RectTransform.anchoredPosition = Vector2.zero;
            card.RectTransform.rotation = Quaternion.identity;
            card.RectTransform.localScale = Vector3.one;
            
            _eventSystem.Subscribe(card);

            return card;
        }

        public void Release(Card card)
        {
            _eventSystem.Unsubscribe(card);
            ObjectPoolManager.Instance.Return(card.gameObject);
        }
    }
}