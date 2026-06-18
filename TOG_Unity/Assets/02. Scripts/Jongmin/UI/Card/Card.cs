using UnityEngine;

namespace Jongmin
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private CardView view;
        [SerializeField] private CardPointer pointer;

        public BattleCardData BattleCardData { get; private set; }
        public CardData CardData { get; private set; }
        public CardType CardType { get; private set; }

        public CardPointer Pointer => pointer;
        public CardView View => view;
        public RectTransform RectTransform => transform as RectTransform;

        private void Awake()
        {
            view ??= GetComponent<CardView>();
            pointer ??= GetComponent<CardPointer>();
            pointer?.SetOwner(this);
        }

        public void SetBattleCardData(BattleCardData battleCardData, CardType cardType = CardType.None)
        {
            BattleCardData = battleCardData;
            CardData = battleCardData?.data;
            view.UpdateModel(CardData);
            
            CardType = cardType;
        }

        public void SetCardData(CardData cardData, CardType cardType = CardType.None)
        {
            BattleCardData = null;
            CardData = cardData;
            view.UpdateModel(CardData);
            
            CardType = cardType;
        }

        private void OnDisable()
        {
            BattleCardData = null;
            CardData = null;
            CardType = CardType.None;
        }
    }
}
