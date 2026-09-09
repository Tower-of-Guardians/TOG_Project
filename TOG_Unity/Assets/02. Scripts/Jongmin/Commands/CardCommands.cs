using System.Collections.Generic;
using System.Linq;
using JxModule.Terminal;
using UnityEngine;

namespace Jongmin
{
    public sealed class CardCommands
    {
        private readonly HandDomain _handDomain;
        private readonly FieldDomain _fieldDomain;
        private readonly EventDomain _eventDomain;

        public CardCommands(HandDomain handDomain, FieldDomain fieldDomain, EventDomain eventDomain)
        {
            _handDomain = handDomain;
            _fieldDomain = fieldDomain;
            _eventDomain = eventDomain;
        }

        [JxCommand("Card List")]
        private IEnumerable<string> ListCards()
        {
            return DataCenter.card_datas.Keys.ToList();
        }

        [JxCommand("Add Card")]
        private void AddCard([JxOptionValue("Card List")] string cardId)
        {
            DataCenter.Instance.userDeck.Add(DataCenter.card_datas[cardId]);
            // TODO: 이벤트를 통해 인벤토리 갱신을 해야함.
            Debug.Log($"인벤토리에 {DataCenter.card_datas[cardId].name}을 추가했습니다.");
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
            _eventDomain.RecordGainedCard(battleCardData.data);
            Debug.Log($"핸드에 {battleCardData.data.name}을 추가했습니다.");
        }

        [JxCommand("Remove Hand")]
        private void RemoveHand([JxOptionValue("Card List")] string cardId)
        {
            var targetCard = _handDomain.Container.Cards.FirstOrDefault(card => card.CardData.id == cardId);
            _handDomain.System.RemoveCard(targetCard);
            
            if (targetCard != null)
            {
                Debug.Log($"핸드에서 {targetCard.CardData.name}를 제거했습니다.");
            }
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
            _eventDomain.RecordGainedCard(battleCardData.data);
            Debug.Log($"공격 필드에 {battleCardData.data.name}을 추가했습니다.");
        }

        [JxCommand("Remove ATK Field")]
        private void RemoveAtkField([JxOptionValue("Card List")] string cardId)
        {
            var targetCard = _fieldDomain.AtkContainer.Cards.FirstOrDefault(card => card.CardData.id == cardId);
            _fieldDomain.AtkSystem.RemoveCard(targetCard);
            
            if (targetCard != null)
            {
                Debug.Log($"공격 필드에서 {targetCard.CardData.name}를 제거했습니다.");
            }
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
            _eventDomain.RecordGainedCard(battleCardData.data);
            Debug.Log($"방어 필드에 {battleCardData.data.name}을 추가했습니다.");
        }

        [JxCommand("Remove DEF Field")]
        private void RemoveDefField([JxOptionValue("Card List")] string cardId)
        {
            var targetCard = _fieldDomain.DefContainer.Cards.FirstOrDefault(card => card.CardData.id == cardId);
            _fieldDomain.DefSystem.RemoveCard(targetCard);

            if (targetCard != null)
            {
                Debug.Log($"방어 필드에서 {targetCard.CardData.name}를 제거했습니다.");
            }
        }
    }
}