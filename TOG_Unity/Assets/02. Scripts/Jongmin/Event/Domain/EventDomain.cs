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

        private EventHistory _eventHistory;
        private NPCDialogueHistory _dialogueHistory;

        public EventDialogueSystem EventDialogueSystem => eventDialogueSystem;
        public RegularDialogueSystem RegularDialogueSystem => regularDialogueSystem;
        public EventRewardSystem RewardSystem => eventRewardSystem;
        public IEventProgress EventProgress => _eventHistory;
        public IDialogueProgress DialogueProgress => _dialogueHistory;

        public void Construct()
        {
            _eventDataTable = DataTableManager.FindTable<EventDataTableRow>("DT_Event");
            _eventConditionDataTable = DataTableManager.FindTable<EventConditionDataTableRow>("DT_EventCondition");
            _eventRewardDataTable = DataTableManager.FindTable<EventRewardDataTableRow>("DT_EventReward");
            _dialogueEntryDataTable = DataTableManager.FindTable<DialogueEntryDataTableRow>("DT_DialogueEntry");

            _eventHistory = new EventHistory();
            _dialogueHistory = new NPCDialogueHistory();

            eventConditionSystem.Construct(
                _eventConditionDataTable,
                _dialogueHistory,
                _eventHistory,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            eventDialogueSystem.Construct(dialogueRunner, this);
            regularDialogueSystem.Construct(dialogueRunner, _dialogueHistory, _dialogueEntryDataTable);
            eventRewardSystem.Construct(_eventRewardDataTable);

            BindEvents();
        }

        public void BindEvents()
        {
            eventDialogueSystem.OnBeginEventReward += HandleBeginEventReward;
        }

        public void ReleaseEvents()
        {
            eventDialogueSystem.OnBeginEventReward -= HandleBeginEventReward;
        }

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

        public bool TryStartDialogue(string npcID)
        {
            if (TryStartEventDialogue(npcID))
            {
                return true;
            }

            return TryStartRegularDialogue(npcID);
        }

        public bool TryStartEventDialogue(string npcID)
        {
            return eventDialogueSystem != null && eventDialogueSystem.TryStartEventDialogue(npcID);
        }

        public bool TryStartRegularDialogue(string npcID)
        {
            return regularDialogueSystem != null && regularDialogueSystem.TryStartRegularDialogue(npcID);
        }

        public EventDataTableRow FindRunnableEvent(string npcID)
        {
            if (string.IsNullOrWhiteSpace(npcID) || _eventDataTable == null)
            {
                return null;
            }

            return _eventDataTable
                .FindAll<EventDataTableRow>()
                .Where(row => row.npcID == npcID)
                .Where(row => row.isEnable)
                .Where(row => !row.isOnce || !_eventHistory.HasSeen(row.rowID))
                .Where(row => eventConditionSystem == null || eventConditionSystem.IsSatisfied(row.conditionIDs, npcID))
                .OrderBy(row => row.priority)
                .FirstOrDefault();
        }

        public void MarkEventSeen(EventDataTableRow eventDataTableRow)
        {
            if (eventDataTableRow == null || !eventDataTableRow.isOnce)
            {
                return;
            }

            _eventHistory.MarkSeen(eventDataTableRow.rowID);
        }

        private void HandleBeginEventReward(EventDataTableRow eventDataTableRow)
        {
            if (eventDataTableRow == null)
            {
                return;
            }

            eventRewardSystem.Execute(eventDataTableRow.rewardIDs, eventDataTableRow.npcID);
        }

        private void OnDestroy()
        {
            ReleaseEvents();
        }
    }
}
