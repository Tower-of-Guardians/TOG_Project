using System;
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
        [SerializeField] private RegularDialogueSystem regularDialogueSystem;
        [SerializeField] private EventConditionSystem eventConditionSystem;
        [SerializeField] private EventRewardSystem eventRewardSystem;

        private DataTable _eventDataTable;
        private DataTable _eventConditionDataTable;
        private DataTable _eventRewardDataTable;
        private DataTable _dialogueEntryDataTable;

        private EventProgressCache _progressCache;

        public EventDialogueSystem EventDialogueSystem => eventDialogueSystem;
        public RegularDialogueSystem RegularDialogueSystem => regularDialogueSystem;
        public EventRewardSystem RewardSystem => eventRewardSystem;
        public EventProgressCache ProgressCache => _progressCache;
        public RunEventProgress RunProgress => _progressCache?.Run;
        public GlobalEventProgress GlobalProgress => _progressCache?.Global;
        public IEventProgress EventProgress => _progressCache?.Global;
        public IDialogueProgress DialogueProgress => _progressCache?.Global;
        
        public void Construct()
        {
            _eventDataTable = DataTableManager.FindTable<EventDataTableRow>("DT_Event");
            _eventConditionDataTable = DataTableManager.FindTable<EventConditionDataTableRow>("DT_EventCondition");
            _eventRewardDataTable = DataTableManager.FindTable<EventRewardDataTableRow>("DT_EventReward");
            _dialogueEntryDataTable = DataTableManager.FindTable<DialogueEntryDataTableRow>("DT_DialogueEntry");

            _progressCache = new EventProgressCache();

            eventConditionSystem.Construct(
                _eventConditionDataTable,
                _progressCache.Global,
                _progressCache.Global,
                null,
                _progressCache.Run,
                _progressCache.Run,
                null,
                _progressCache.CardInventory,
                _progressCache.Run,
                _progressCache.Run);

            eventDialogueSystem.Construct(dialogueRunner);
            regularDialogueSystem.Construct(dialogueRunner, _progressCache.Global, _dialogueEntryDataTable);
            eventRewardSystem.Construct(_eventRewardDataTable);

            BindEvents();
        }
        
        public void BindEvents()
        {
            if (GameData.Instance == null)
            {
                return;
            }

            GameData.Instance.SynergyChange += HandleSynergyChanged;
            HandleSynergyChanged(GameData.Instance.synergyIDList);
        }
        
        public void ReleaseEvents()
        {
            if (GameData.Instance == null)
            {
                return;
            }

            GameData.Instance.SynergyChange -= HandleSynergyChanged;
        }

        /// <summary>
        /// 현재 런에서만 유지되는 이벤트 진행도 캐시를 초기화합니다.
        /// 런 시작 또는 런 종료 후 새 런을 준비할 때 호출합니다.
        /// </summary>
        public void ResetRunProgress()
        {
            _progressCache?.ResetRun();
        }

        /// <summary>
        /// 현재 런에서 도달한 스테이지를 기록합니다.
        /// 스테이지 진입이 확정되는 시점에 호출합니다.
        /// </summary>
        public void RecordReachedStage(int stage)
        {
            _progressCache?.Run.RecordReachedStage(stage);
        }

        /// <summary>
        /// 현재 런에서 한 번의 공격으로 준 최대 피해량을 기록합니다.
        /// 글로벌 최대 피해량도 함께 갱신합니다.
        /// </summary>
        public void RecordSingleAttackDamage(int damage)
        {
            _progressCache?.Run.RecordSingleAttackDamage(damage);
            _progressCache?.Global.RecordSingleAttackDamage(damage);
        }

        /// <summary>
        /// 현재 런에서 얻은 카드의 개수를 성마다 기록합니다.
        /// 카드가 실제로 유저 덱에 추가된 직후 호출합니다.
        /// </summary>
        public void RecordGainedCard(CardData cardData)
        {
            _progressCache?.Run.RecordGainedCard(cardData);
        }

        /// <summary>
        /// NPC 상호작용을 처리합니다.
        /// 실행 가능한 이벤트 대화가 있으면 이벤트 대화를 우선 실행하고,
        /// 이벤트 대화가 없으면 정규 대화를 시도합니다.
        /// 실행 가능한 대화가 없을 때만 NPC 고유 행동을 실행합니다.
        /// </summary>
        /// <returns>이벤트 대화, 정규 대화, 고유 행동 중 하나라도 실행했다면 true입니다.</returns>
        public bool TryInteract(string npcID, Action specialAction = null)
        {
            if (TryStartDialogue(npcID))
            {
                return true;
            }

            if (specialAction == null)
            {
                return false;
            }

            specialAction.Invoke();
            return true;
        }

        /// <summary>
        /// NPC와 진행 가능한 대화를 시작합니다.
        /// 이벤트 대화를 먼저 검사하고, 이벤트 대화가 없을 때만 정규 대화를 검사합니다.
        /// </summary>
        private bool TryStartDialogue(string npcID)
        {
            if (TryStartEventDialogue(npcID))
            {
                return true;
            }

            return TryStartRegularDialogue(npcID);
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
        /// NPC와 진행 가능한 정규 대화를 시작합니다.
        /// 현재 NPC 대화 step에 매칭되는 대화가 있을 때만 실행합니다.
        /// </summary>
        private bool TryStartRegularDialogue(string npcID)
        {
            return regularDialogueSystem != null && regularDialogueSystem.TryStartRegularDialogue(npcID);
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
                .Where(row => !row.isOnce || _progressCache == null || !_progressCache.Global.HasSeen(row.rowID))
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

            _progressCache?.Global.MarkSeen(eventDataTableRow.rowID);
        }

        private void HandleBeginEventReward(EventDataTableRow eventDataTableRow)
        {
            if (eventDataTableRow == null)
            {
                return;
            }

            eventRewardSystem.Execute(eventDataTableRow.rewardIDs, eventDataTableRow.npcID);
        }

        private void HandleSynergyChanged(System.Collections.Generic.Dictionary<string, SynergyTotalData> synergyMap)
        {
            _progressCache?.Run.RefreshSynergyRecords(synergyMap);
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}
