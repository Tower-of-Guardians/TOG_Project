using UnityEngine;

namespace Jongmin
{
    public class CardInvenDomain : MonoBehaviour
    {
        [SerializeField] private CardInvenView cardInvenView;
        [SerializeField] private CardInvenSystem cardInvenSystem;
        [SerializeField] private CardInvenEventSystem cardInvenEventSystem;

        [SerializeField] private CardInfoDomain cardInfoDomain;
        
        private CardContainer _cardContainer;
        private InventoryCardFactory _cardFactory;
        
        public CardInvenSystem System => cardInvenSystem;
        
        public void Construct()
        {
            _cardContainer = new CardContainer();
            _cardFactory = new InventoryCardFactory(cardInvenView, cardInvenEventSystem);
            
            cardInvenSystem.Construct(cardInvenView, _cardContainer, _cardFactory);
            
            BindEvents();
        }

        public void BindEvents()
        {
            cardInvenEventSystem.RequestShowCardInfo += HandleRequestShowCardInfo;
            cardInvenSystem.RequestCloseView += cardInfoDomain.System.CloseView;
        }

        public void ReleaseEvents()
        {
            cardInvenEventSystem.RequestShowCardInfo -= HandleRequestShowCardInfo;
            cardInvenSystem.RequestCloseView -= cardInfoDomain.System.CloseView;
        }

        private void HandleRequestShowCardInfo(CardData cardData)
        {
            cardInfoDomain.System.OpenView(cardData);
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}