using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class FieldCardFactory
    {
        private readonly FieldView _view;
        private readonly FieldEventSystem _eventSystem;
        private readonly Card _prefab;

        public FieldCardFactory(FieldView view, FieldEventSystem eventSystem)
        {
            _view = view;
            _eventSystem = eventSystem;
            _prefab = PrefabManager.CachePrefab<Card>("PF_Card");
        }

        public Card Create()
        {
            var cardObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            cardObject.transform.SetParent(_view.CardRoot, false);
            cardObject.transform.localScale = 0.75f * Vector3.one;

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

