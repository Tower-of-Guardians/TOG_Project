using System;
using System.Collections.Generic;
using JxModule.DataTable;
using UnityEngine;

namespace Jongmin
{
    public class EventRewardSystem : MonoBehaviour
    {
        private DataTable _eventRewardDataTable;

        public event Action<string> OnRewardRelic;
        public event Action<string> OnRewardCard;
        public event Action<int> OnRewardGold;
        public event Action<int> OnRewardHeal;

        public void Construct(DataTable eventRewardDataTable)
        {
            _eventRewardDataTable = eventRewardDataTable;
        }

        public void Execute(List<string> rewardIDs, string npcID)
        {
            if (_eventRewardDataTable == null)
            {
                return;
            }

            if (rewardIDs == null)
            {
                return;
            }

            foreach (var rewardID in rewardIDs)
            {
                Execute(rewardID, npcID);
            }
        }

        private void Execute(string rewardID, string npcID)
        {
            if (string.IsNullOrWhiteSpace(rewardID))
            {
                return;
            }

            var row = _eventRewardDataTable.Find<EventRewardDataTableRow>(rewardID);
            if (row == null)
            {
                return;
            }

            switch (row.rewardType)
            {
                case EEventRewardType.Relic:
                    OnRewardRelic?.Invoke(row.targetID);
                    break;
                
                case EEventRewardType.Card:
                    OnRewardCard?.Invoke(row.targetID);
                    break;
                
                case EEventRewardType.Gold:
                    OnRewardGold?.Invoke(row.amount);
                    break;
                
                case EEventRewardType.Heal:
                    OnRewardHeal?.Invoke(row.amount);
                    break;
            }
        }
    }
}
