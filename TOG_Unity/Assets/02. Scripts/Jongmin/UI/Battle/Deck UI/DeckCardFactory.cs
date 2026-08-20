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
            var cardObject = JxModule.ObjectPoolManager.Instance.Get(_prefab.gameObject);
            cardObject.transform.SetParent(_view.CardRoot, false);
            
            var card = cardObject.GetComponent<Card>();
            card.ResetRectTransform(Vector3.one);
            return card;
        }

        public void Release(Card card)
        {
            JxModule.ObjectPoolManager.Instance.Return(card.gameObject);
        }
    }
}
