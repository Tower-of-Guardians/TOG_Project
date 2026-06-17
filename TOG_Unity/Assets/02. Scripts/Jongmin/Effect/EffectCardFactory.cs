using JxModule;
using UnityEngine;

namespace Jongmin
{
    public class EffectCardFactory
    {
        private readonly RectTransform _cardRoot;
        private readonly Card _prefab;

        public EffectCardFactory(RectTransform cardRoot)
        {
            _cardRoot = cardRoot;
            _prefab = PrefabManager.CachePrefab<Card>("PF_Card");
        }
        
        public Card Create()
        {
            var cardObject = ObjectPoolManager.Instance.Get(_prefab.gameObject);
            cardObject.transform.SetParent(_cardRoot, false);
            cardObject.transform.localPosition = Vector3.zero;
            cardObject.transform.localRotation = Quaternion.identity;
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