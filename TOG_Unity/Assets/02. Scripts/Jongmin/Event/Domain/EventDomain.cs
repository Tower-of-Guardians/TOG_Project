using System;
using System.Collections.Generic;
using System.Linq;
using JxDialogueBox;
using JxModule;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class EventDomain : MonoBehaviour
    {
        [BigHeader("Inner References")]
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private EventDialogueSystem eventDialogueSystem;
        [SerializeField] private EventConditionSystem eventConditionSystem;
        [SerializeField] private EventRewardSystem eventRewardSystem;

        private RelicDomain _relicDomain;
        
        private DataTable _eventDataTable;
        private DataTable _eventConditionDataTable;
        private DataTable _eventRewardDataTable;

        private EventProgressCache _progressCache;
        private readonly NpcEncounterSession _npcEncounterSession = new();

        public EventDialogueSystem EventDialogueSystem => eventDialogueSystem;
        public EventRewardSystem RewardSystem => eventRewardSystem;
        public EventProgressCache ProgressCache => _progressCache;
        public RunEventProgress RunProgress => _progressCache?.Run;
        public SlotEventProgress SlotProgress => _progressCache?.Slot;
        public IEventProgress EventProgress => _progressCache?.Slot;
        public bool IsDialoguePlaying => eventDialogueSystem != null && eventDialogueSystem.IsPlaying;

        public void Construct(RelicDomain relicDomain)
        {
            _relicDomain = relicDomain;
            
            _eventDataTable = DataTableManager.FindTable<EventDataTableRow>("DT_Event");
            _eventConditionDataTable = DataTableManager.FindTable<EventConditionDataTableRow>("DT_EventCondition");
            _eventRewardDataTable = DataTableManager.FindTable<EventRewardDataTableRow>("DT_EventReward");

            _progressCache = new EventProgressCache();

            eventConditionSystem.Construct(
                _eventConditionDataTable,
                _progressCache.Slot,
                null,
                _progressCache.Run,
                _progressCache.Run,
                relicDomain,
                _progressCache.CardInventory,
                _progressCache.Run,
                _progressCache.Run,
                _progressCache.Slot.Experience,
                _progressCache.Run.Experience);

            eventDialogueSystem.Construct(dialogueRunner);
            eventRewardSystem.Construct(_eventRewardDataTable);

            BindEvents();
            RunSaveService.Bind(this, relicDomain);
        }
        
        public void BindEvents()
        {
            GameData.Instance.SynergyChange += HandleSynergyChanged;
            eventRewardSystem.OnRewardRelic += HandleOnRewardRelic;
            if (_relicDomain != null)
            {
                _relicDomain.OnRelicAdded += HandleRelicAdded;
            }
            HandleSynergyChanged(GameData.Instance.synergyIDList);
        }
        
        public void ReleaseEvents()
        {
            GameData.Instance.SynergyChange -= HandleSynergyChanged;
            eventRewardSystem.OnRewardRelic -= HandleOnRewardRelic;
            if (_relicDomain != null)
            {
                _relicDomain.OnRelicAdded -= HandleRelicAdded;
            }
        }

        /// <summary>
        /// 현재 런에서만 유지되는 이벤트 진행도 캐시를 초기화합니다.
        /// 런 시작 또는 런 종료 후 새 런을 준비할 때 호출합니다.
        /// </summary>
        public void ResetRunProgress()
        {
            _progressCache?.ResetRun();
            _npcEncounterSession.Clear();
        }

        /// <summary>
        /// NPC 조우 이벤트에 진입한 시점에 호출합니다.
        /// TryInteract와 분리되어 있으며, 이번 런의 NPC 조우 횟수와 이번 조우 세션을 시작합니다.
        /// </summary>
        public void RecordNpcEncounter(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }

            _progressCache?.Run.RecordNpcEncounter(npcID);
            _progressCache?.Slot.AddExperience(EExperienceRecordType.NpcEncounter, npcID);
            SaveSlotProgressIfOutsideRun();
            _npcEncounterSession.Begin(npcID);
        }
        
        /// <summary>
        /// [Debug] NPC 만남 횟수를 설정합니다.
        /// </summary>
        public void SetNpcEncounter(string npcID, int count)
        {
            if (string.IsNullOrWhiteSpace(npcID))
            {
                return;
            }

            _progressCache?.Run.SetNpcEncounter(npcID, count);
        }

        /// <summary>
        /// NPC 조우 이벤트가 종료되는 시점에 호출합니다.
        /// 이번 조우에서 사용한 특수 행동 플래그를 정리합니다.
        /// </summary>
        public void EndNpcEncounter(string npcID)
        {
            _npcEncounterSession.End(npcID);
        }

        /// <summary>
        /// 현재 런에서 도달한 스테이지를 기록합니다.
        /// 스테이지 진입이 확정되는 시점에 호출합니다.
        /// </summary>
        public void RecordReachedStage(int stage)
        {
            _progressCache?.Run.RecordReachedStage(stage);
            _progressCache?.Slot.MarkExperience(EExperienceRecordType.StageReached, stage.ToString());
        }

        /// <summary>
        /// 현재 런에서 한 번의 공격으로 준 최대 피해량을 기록합니다.
        /// 슬롯 최대 피해량도 함께 갱신합니다.
        /// </summary>
        public void RecordSingleAttackDamage(int damage)
        {
            _progressCache?.Run.RecordSingleAttackDamage(damage);
            _progressCache?.Slot.RecordSingleAttackDamage(damage);
        }

        /// <summary>
        /// 현재 런에서 얻은 카드의 개수를 성마다 기록합니다.
        /// 카드가 실제로 유저 덱에 추가된 직후 호출합니다.
        /// </summary>
        public void RecordGainedCard(CardData cardData)
        {
            _progressCache?.Run.RecordGainedCard(cardData);
            if (cardData != null)
            {
                _progressCache?.Slot.AddExperience(EExperienceRecordType.CardGained, cardData.id);
                _progressCache?.Slot.AddExperience(EExperienceRecordType.CardGained, "Grade", cardData.grade.ToString());
                _progressCache?.Slot.AddExperience(EExperienceRecordType.CardGained, "Star", cardData.star.ToString());
            }
        }

        public void RecordRunResult(ERunEndReason reason)
        {
            _progressCache?.Slot.AddExperience(EExperienceRecordType.RunResult, reason.ToString());
            if (reason == ERunEndReason.Abandon)
            {
                _progressCache?.Slot.AddExperience(EExperienceRecordType.RunResult, ERunEndReason.Death.ToString());
            }
        }

        public void RecordMonsterKill(string monsterID)
        {
            if (string.IsNullOrWhiteSpace(monsterID))
            {
                return;
            }
            _progressCache?.Run.ExperienceRecordAdd(EExperienceRecordType.MonsterKill, monsterID);
            _progressCache?.Slot.AddExperience(EExperienceRecordType.MonsterKill, monsterID);
        }

        public void RecordEventExperience(string eventID, string choiceID = null)
        {
            if (string.IsNullOrWhiteSpace(eventID))
            {
                return;
            }
            _progressCache?.Run.ExperienceRecordAdd(EExperienceRecordType.EventExperienced, eventID, choiceID);
            _progressCache?.Slot.AddExperience(EExperienceRecordType.EventExperienced, eventID, choiceID);
        }

        public SlotProgressSaveData CaptureSlotProgress()
            => _progressCache?.Slot.Capture(_relicDomain?.OwnedRelicIDs) ?? new SlotProgressSaveData();

        public List<ExperienceRecordData> CaptureRunProgress()
            => _progressCache?.Run.Capture() ?? new System.Collections.Generic.List<ExperienceRecordData>();

        public void RestoreProgress(SlotProgressSaveData persistent, System.Collections.Generic.IEnumerable<ExperienceRecordData> run)
        {
            _progressCache?.Slot.Restore(persistent);
            _progressCache?.Run.Restore(run);
            _relicDomain?.RestoreOwnedRelics(persistent?.relicIDs);
        }

        /// <summary>
        /// NPC 상호작용을 처리합니다.
        /// 실행 가능한 이벤트 대화가 있으면 이벤트 대화를 우선 실행합니다.
        /// 이벤트 대화가 없을 때만 이번 조우에서 NPC 고유 행동을 1회 실행합니다.
        /// </summary>
        /// <returns>이벤트 대화 또는 고유 행동 중 하나라도 실행했다면 true입니다.</returns>
        public bool TryInteract(string npcID, Action specialAction = null)
        {
            if (TryStartEventDialogue(npcID))
            {
                return true;
            }

            if (specialAction == null)
            {
                return false;
            }

            if (!_npcEncounterSession.CanUseSpecialAction(npcID))
            {
                return false;
            }

            _npcEncounterSession.MarkSpecialActionUsed(npcID);
            specialAction.Invoke();
            return true;
        }

        /// <summary>
        /// NPC에게서 현재 실행 가능한 이벤트가 있는지 확인하고, 해당 이벤트의 대표 보상 타입을 반환합니다.
        /// </summary>
        public bool TryGetEvent(string npcID, out EEventRewardType rewardType)
        {
            rewardType = EEventRewardType.None;

            var eventDataTableRow = FindRunnableEvent(npcID);
            if (eventDataTableRow == null)
            {
                return false;
            }

            rewardType = GetEventRewardType(eventDataTableRow);
            return true;
        }

        /// <summary>
        /// NPC와 진행 가능한 이벤트 대화를 시작합니다.
        /// 이벤트 조건과 이미 본 이벤트 여부는 내부에서 검사합니다.
        /// </summary>
        private bool TryStartEventDialogue(string npcID)
        {
            if (eventDialogueSystem == null)
            {
                return false;
            }

            var eventDataTableRow = FindRunnableEvent(npcID);
            if (eventDataTableRow == null)
            {
                return false;
            }

            return eventDialogueSystem.StartEventDialogue(eventDataTableRow, () =>
            {
                HandleBeginEventReward(eventDataTableRow);
                MarkEventSeen(eventDataTableRow);
            });
        }

        /// <summary>
        /// NPC에게서 현재 실행 가능한 이벤트 행을 찾습니다.
        /// 같은 NPC의 이벤트 중 활성화 상태, 일회성 실행 여부, 조건 만족 여부를 검사한 뒤 우선순위가 가장 낮은 이벤트를 반환합니다.
        /// </summary>
        private EventDataTableRow FindRunnableEvent(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID) || _eventDataTable == null)
            {
                return null;
            }

            return _eventDataTable
                .FindAll<EventDataTableRow>()
                .Where(row => row.npcID == npcID)
                .Where(row => row.isEnable)
                .Where(row => !row.isOnce || _progressCache == null || !_progressCache.Slot.HasSeen(row.rowID))
                .Where(row => eventConditionSystem == null || eventConditionSystem.IsSatisfied(row.conditionIDs, npcID))
                .OrderBy(row => row.priority)
                .FirstOrDefault();
        }

        /// <summary>
        /// 일회성 이벤트를 이미 실행했던 이벤트로 기록합니다.
        /// 대화가 끝난 뒤 이벤트 재실행을 막기 위해 호출합니다.
        /// </summary>
        private void MarkEventSeen(EventDataTableRow eventDataTableRow)
        {
            if (eventDataTableRow == null || !eventDataTableRow.isOnce)
            {
                return;
            }

            _progressCache?.Slot.MarkSeen(eventDataTableRow.rowID);
            SaveSlotProgressIfOutsideRun();
        }

        private void HandleBeginEventReward(EventDataTableRow eventDataTableRow)
        {
            if (eventDataTableRow == null)
            {
                return;
            }

            eventRewardSystem.Execute(eventDataTableRow.rewardIDs, eventDataTableRow.npcID);
        }

        private EEventRewardType GetEventRewardType(EventDataTableRow eventDataTableRow)
        {
            if (eventDataTableRow?.rewardIDs == null || _eventRewardDataTable == null)
            {
                return EEventRewardType.None;
            }

            foreach (var rewardID in eventDataTableRow.rewardIDs)
            {
                if (string.IsNullOrWhiteSpace(rewardID))
                {
                    continue;
                }

                var rewardDataTableRow = _eventRewardDataTable.Find<EventRewardDataTableRow>(rewardID);
                if (rewardDataTableRow == null || !rewardDataTableRow.isEnable)
                {
                    continue;
                }

                return rewardDataTableRow.rewardType;
            }

            return EEventRewardType.None;
        }

        private void HandleSynergyChanged(System.Collections.Generic.Dictionary<string, SynergyTotalData> synergyMap)
        {
            _progressCache?.Run.RefreshSynergyRecords(synergyMap);
            if (synergyMap == null)
            {
                return;
            }
            foreach (var pair in synergyMap)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
                {
                    continue;
                }
                for (int count = 1; count <= pair.Value.count; count++)
                {
                    _progressCache?.Slot.MarkExperience(EExperienceRecordType.SynergyActivated, pair.Key, count.ToString());
                }
            }
        }
        
#region Event Handlings
        private void HandleOnRewardRelic(string relicID)
        {
            _relicDomain.TryAddRelic(relicID);
        }

        private void HandleRelicAdded(RelicDataTableRow relic)
        {
            if (relic == null)
            {
                return;
            }
            _progressCache?.Run.ExperienceRecordAdd(EExperienceRecordType.RelicGained, relic.rowID);
            _progressCache?.Slot.AddExperience(EExperienceRecordType.RelicGained, relic.rowID);
            SaveSlotProgressIfOutsideRun();
        }

        private static void SaveSlotProgressIfOutsideRun()
        {
            if (!RunSaveService.HasActiveRun)
            {
                RunSaveService.SaveSlotProgress();
            }
        }
#endregion

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}
