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

        [JxCommand("Begin Npc Encounter")]
        private string BeginNpcEncounter([JxOptionValue("NPC List")] string npcId)
        {
            _eventDomain.RecordNpcEncounter(npcId);
            return $"{npcId}과(와) 조우했습니다.";
        }

        [JxCommand("End Npc Encounter")]
        private string EndNpcEncounter([JxOptionValue("NPC List")] string npcId)
        {
            _eventDomain.EndNpcEncounter(npcId);
            return $"{npcId}과(와) 헤어졌습니다.";
        }

        [JxCommand("Try Interact")]
        private string TryInteract([JxOptionValue("NPC List")] string npcId)
        {


            var isSuccess = _eventDomain.TryInteract(npcId, () =>
            {
                Debug.Log($"{npcId}의 고유 행동을 실행합니다.");
            });

            return $"TryInteract({npcId}) => {isSuccess}";
        }

        [JxCommand("NPC List")]
        private IEnumerable<string> ListNpc()
        {
            return DataTableManager
                .FindAllRows<CharacterDataTableRow>()
                .Where(x => x.isEnable)
                .Select(x => x.rowID)
                .OrderBy(x => x);
        }
    }
}
