using System.Collections.Generic;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class EventConditionSystem : MonoBehaviour
    {
        private DataTable _eventConditionDataTable;

        private IEventProgress _eventProgress;
        private IShopProgress _shopProgress;
        private IRunProgress _runProgress;
        private IBattleRecord _battleRecord;
        private IRelicInventory _relicInventory;
        private ICardInventory _cardInventory;
        private IRunCardRecord _runCardRecord;
        private ISynergyRecord _synergyRecord;

        public void Construct(DataTable eventConditionDataTable,
                              IEventProgress eventProgress,
                              IShopProgress shopProgress,
                              IRunProgress runProgress,
                              IBattleRecord battleRecord,
                              IRelicInventory relicInventory,
                              ICardInventory cardInventory,
                              IRunCardRecord runCardRecord,
                              ISynergyRecord synergyRecord)
        {
            _eventConditionDataTable = eventConditionDataTable;
            _eventProgress = eventProgress;
            _shopProgress = shopProgress;
            _runProgress = runProgress;
            _battleRecord = battleRecord;
            _relicInventory = relicInventory;
            _cardInventory = cardInventory;
            _runCardRecord = runCardRecord;
            _synergyRecord = synergyRecord;
        }
        
        public bool IsSatisfied(List<string> conditionIDs, string npcID)
        {
            if (_eventConditionDataTable == null)
            {
                return false;
            }

            if (conditionIDs == null || conditionIDs.Count == 0)
            {
                return true;
            }

            foreach (var conditionID in conditionIDs)
            {
                if (!IsSatisfied(conditionID, npcID))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSatisfied(string conditionID, string npcID)
        {
            if (string.IsNullOrWhiteSpace(conditionID))
            {
                return true;
            }

            var row = _eventConditionDataTable.Find<EventConditionDataTableRow>(conditionID);
            if (row == null)
            {
                return false;
            }

            return row.conditionType switch
            {
                EEventConditionType.FirstNpcEncounter
                    => _runProgress != null && _runProgress.GetNpcEncounterCount(npcID) == 1,
                    
                EEventConditionType.NpcEncounterCountAtLeast
                    => _runProgress != null && _runProgress.GetNpcEncounterCount(npcID) >= row.value,
                
                EEventConditionType.EventSeen
                    => _eventProgress != null && _eventProgress.HasSeen(row.targetID),
                
                EEventConditionType.EventNotSeen
                    => _eventProgress != null && !_eventProgress.HasSeen(row.targetID),
                    
                EEventConditionType.ShopAllItemsPurchased 
                    => _shopProgress != null && _shopProgress.HasPurchasedAllItems(),
                
                EEventConditionType.ReachedStageAtLeast
                    => _runProgress != null && _runProgress.HasReachedStage(row.value),
                
                EEventConditionType.FirstReachedStage 
                    => _runProgress != null && _runProgress.HasFirstReachedStage(row.value),
                
                EEventConditionType.MaxSingleAttackDamageAtLeast 
                    => _battleRecord != null && _battleRecord.MaxSingleAttackDamage >= row.value,
                
                EEventConditionType.HasRelic 
                    => _relicInventory != null && _relicInventory.HasRelic(row.targetID),
                
                EEventConditionType.HasRelicCountAtLeast 
                    => _relicInventory != null && _relicInventory.RelicCount >= row.value,
                
                EEventConditionType.HasCard 
                    => _cardInventory != null && _cardInventory.HasCard(row.targetID),
                
                EEventConditionType.HasCardCountAtLeast 
                    => _cardInventory != null && _cardInventory.CardCount >= row.value,
                
                EEventConditionType.GainedCardGradeCountInRunAtLeast 
                    => _runCardRecord != null && _runCardRecord.GetGainedCountByGrade(ParseInt(row.arguments, 0)) >= row.value,
                
                EEventConditionType.FirstActivatedSynergyAtLeast 
                    => _synergyRecord != null && _synergyRecord.HasFirstActivatedAtLeast(row.targetID, row.value),
                
                EEventConditionType.ActivatedSynergyAtLeast 
                    => _synergyRecord != null && _synergyRecord.HasActivatedAtLeast(row.targetID, row.value),
                
                _ => false
            };
        }

        private static int ParseInt(IReadOnlyList<string> values, int index)
        {
            if (values == null || index < 0 || index >= values.Count)
            {
                return 0;
            }

            return int.TryParse(values[index], out var result) ? result : 0;
        }
    }
}
