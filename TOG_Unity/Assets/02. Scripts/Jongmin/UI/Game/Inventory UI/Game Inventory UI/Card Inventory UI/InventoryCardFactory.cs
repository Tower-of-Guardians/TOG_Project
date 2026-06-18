using UnityEngine;
using JxModule;

namespace Jongmin
{
    public class InventoryCardFactory
    {
        private readonly CardInvenView _view;
        private readonly CardInvenEventSystem _eventSystem;
        private readonly Card _prefab;

        public InventoryCardFactory(CardInvenView view,  CardInvenEventSystem eventSystem)
        {
            _view = view;
            _eventSystem = eventSystem;
            _prefab = PrefabManager.CachePrefab<Card>("PF_Card");
        }

        public Card Create()
        {
            var cardObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            cardObject.transform.SetParent(_view.CardRoot, false);
            cardObject.transform.localRotation = Quaternion.identity;
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