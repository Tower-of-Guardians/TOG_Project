using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class DeckCardFactory
    {
        private readonly DeckView _view;
        private readonly Card _prefab;
    
        public DeckCardFactory(DeckView view)
        {
            _view = view;
            _prefab = PrefabManager.CachePrefab<Card>("PF_Card");
        }

        public Card Create()
        {
            var cardObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            cardObject.transform.SetParent(_view.CardRoot, false);
            cardObject.transform.localScale = Vector3.one;
            
            var card = cardObject.GetComponent<Card>();
            return card;
        }

        public void Release(Card card)
        {
            ObjectPoolManager.Instance.Return(card.gameObject);
        }
    }
}
