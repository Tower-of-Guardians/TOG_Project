using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class DiscardCardFactory
    {
        private readonly DiscardView _view;
        private readonly DiscardEventSystem _eventSystem;
        private readonly Card _prefab;

        public DiscardCardFactory(DiscardView view, DiscardEventSystem eventSystem)
        {
            _view = view;
            _eventSystem = eventSystem;
            _prefab = PrefabManager.CachePrefab<Card>("PF_Card");
        }

        public Card Create()
        {
            var cardObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            cardObject.transform.SetParent(_view.CardRoot, false);
            cardObject.transform.localScale = Vector3.one;
        
            var card = cardObject.GetComponent<Card>();
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