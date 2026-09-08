using System.Collections.Generic;
using System.Linq;
using JxDialogueBox;
using JxModule.DataTable;
using JxModule.Terminal;
using UnityEngine;

namespace Jongmin
{
    public class EventCommands
    {
        private readonly EventDomain _eventDomain;

        public EventCommands(EventDomain eventDomain)
        {
            _eventDomain = eventDomain;
        }
        
        [JxCommand("Record Stage")]
        private void RecordStage(int stage)
        {
            _eventDomain.RecordReachedStage(stage);    
            Debug.Log($"도달한 스테이지를 {stage}로 설정했습니다.");
        }

        [JxCommand("Record SingleATK")]
        private void RecordSingleAtk(int damage)
        {
            _eventDomain.RecordSingleAttackDamage(damage);
            Debug.Log($"단일 공격 최대 피해량을 {damage}로 설정했습니다.");
        }

        [JxCommand("Record Npc Encounter")]
        private void RecordNpcEncounter([JxOptionValue("NPC List")] string npcId, int count)
        {
            _eventDomain.SetNpcEncounter(npcId, count);
            Debug.Log($"{npcId}와의 만남 횟수를 {count}로 설정했습니다.");
        }

        [JxCommand("NPC List")]
        private IEnumerable<string> ListNpc()
        {
            return DataTableManager
                .FindAllRows<CharacterDataTableRow>()
                .Where(x => x.isEnable)
                .Select(x => x.rowID);
        }
    }
}