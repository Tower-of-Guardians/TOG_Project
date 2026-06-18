using UnityEngine;

namespace Jongmin
{
    public class DeckSystem : MonoBehaviour
    {
        private DeckView _view;
        private CardContainer _container;
        private DeckCardFactory _factory;

        public void Construct(DeckView view, CardContainer container, DeckCardFactory factory)
        {
            _view = view;
            _container = container;
            _factory = factory;
        }

        public void OpenDrawView()
        {
            OpenView(DeckType.Draw);
        }

        public void OpenDiscardView()
        {
            OpenView(DeckType.Throw);
        }

        public void CloseView()
        {
            foreach (var card in _container.Cards)
            {
                RemoveCard(card);
            }
            
            _container.Clear();
            _view.Hide();
        }
        
        public void UpdateCardCount(DeckType deckType, int count)
        {
            switch (deckType)
            {
                case DeckType.Draw:
                    _view.UpdateDrawCardCount(count);
                    break;
                
                case DeckType.Throw:
                    _view.UpdateDiscardCardCount(count);
                    break;
            }
        }

        private void CreateCard(BattleCardData battleCardData)
        {
            var card = _factory.Create();
            card.SetBattleCardData(battleCardData, CardType.Deck);
            _container.Add(card);
        }

        private void RemoveCard(Card card)
        {
            _factory.Release(card);
        }

        private void OpenView(DeckType deckType)
        {
            var titleName = GetTitleName(deckType);
            _view.Show(titleName);

            var battleCardDatas = GameData.Instance.GetDeckDatas(deckType);
            foreach (var battleCardData in battleCardDatas)
            {
                CreateCard(battleCardData);
            }
        }

        private static string GetTitleName(DeckType deckType)
        {
            return deckType switch
            {
                DeckType.Draw => "미사용 카드 더미",
                DeckType.Throw => "사용한 카드 더미",
                _ => string.Empty
            };
        }
    }
}