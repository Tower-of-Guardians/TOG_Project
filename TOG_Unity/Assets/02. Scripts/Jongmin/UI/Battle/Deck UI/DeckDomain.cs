using UnityEngine;

namespace Jongmin
{
    public class DeckDomain : MonoBehaviour
    {
        [SerializeField] private DeckView deckView;
        [SerializeField] private DeckSystem deckSystem;

        private CardContainer _cardContainer;
        private DeckCardFactory _cardFactory;
        
        public void Construct()
        {
            _cardContainer = new CardContainer();
            _cardFactory = new DeckCardFactory(deckView);
            
            deckSystem.Construct(deckView, _cardContainer, _cardFactory);
            
            BindEvents();
            
            GameData.Instance.InvokeDeckCountChange(DeckType.Draw);
            GameData.Instance.InvokeDeckCountChange(DeckType.Throw);
        }

        public void BindEvents()
        {
            deckView.Bind(deckSystem);
            
            GameData.Instance.DeckChange += deckSystem.UpdateCardCount;
        }
    }
}