using System.Collections.Generic;
using System.Linq;
using JxModule.Terminal;

namespace Jongmin
{
    public sealed class CardCommands
    {
        private readonly HandDomain _handDomain;
        private readonly FieldDomain _fieldDomain;

        public CardCommands(HandDomain handDomain, FieldDomain fieldDomain)
        {
            _handDomain = handDomain;
            _fieldDomain = fieldDomain;
        }

        [JxCommand("Card List")]
        private IEnumerable<string> ListCards()
        {
            return DataCenter.card_datas.Keys.ToList();
        }

        [JxCommand("Add Hand")]
        private void AddHand([JxOptionValue("Card List")] string cardId)
        {
            var battleCardData = new BattleCardData
            {
                index = 0,
                data = DataCenter.card_datas[cardId]
            };
            
            _handDomain.System.CreateCard(battleCardData);
        }

        [JxCommand("Remove Hand")]
        private void RemoveHand([JxOptionValue("Card List")] string cardId)
        {
            var targetCard = _handDomain.Container.Cards.FirstOrDefault(card => card.CardData.id == cardId);
            _handDomain.System.RemoveCard(targetCard);
        }

        [JxCommand("Add ATK Field")]
        private void AddAtkField([JxOptionValue("Card List")] string cardId)
        {
            var battleCardData = new BattleCardData
            {
                index = 0,
                data = DataCenter.card_datas[cardId]
            };

            _fieldDomain.AtkSystem.CreateCard(battleCardData);
        }

        [JxCommand("Remove ATK Field")]
        private void RemoveAtkField([JxOptionValue("Card List")] string cardId)
        {
            var targetCard = _fieldDomain.AtkContainer.Cards.FirstOrDefault(card => card.CardData.id == cardId);
            _fieldDomain.AtkSystem.RemoveCard(targetCard);
        }

        [JxCommand("Add DEF Field")]
        private void AddDefField([JxOptionValue("Card List")] string cardId)
        {
            var battleCardData = new BattleCardData
            {
                index = 0,
                data = DataCenter.card_datas[cardId]
            };

            _fieldDomain.DefSystem.CreateCard(battleCardData);
        }

        [JxCommand("Remove DEF Field")]
        private void RemoveDefField([JxOptionValue("Card List")] string cardId)
        {
            var targetCard = _fieldDomain.DefContainer.Cards.FirstOrDefault(card => card.CardData.id == cardId);
            _fieldDomain.DefSystem.RemoveCard(targetCard);
        }
    }
}