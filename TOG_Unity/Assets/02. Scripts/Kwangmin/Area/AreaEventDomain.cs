using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kwangmin
{
    public class AreaEventDomain : MonoBehaviour
    {
        [SerializeField] private AreaEventUI areaEventUI;

        [Header("Runtime Status")]
        [SerializeField] private string currentAreaEventId = "AreaEvent_01";
        [SerializeField] private PlayerEventStatus playerStatus;

        public event Action<AreaEventType> OnAreaEventSelected;

        private void Start()
        {
            if (areaEventUI != null)
            {
                areaEventUI.Bind(HandleEventSelected);
            }
        }

        public void OpenView(string areaEventId = null)
        {
            if (!string.IsNullOrEmpty(areaEventId))
            {
                currentAreaEventId = areaEventId;
            }

            StartCoroutine(OpenViewRoutine());
        }

        public void ResetStageCounts()
        {
            playerStatus.ResetStageCounts();
        }

        private IEnumerator OpenViewRoutine()
        {
            List<AreaEventType> choices = GetChoices();
            string title = GetAreaTitle();

            playerStatus.DecreaseBlessingCooldown();

            if (areaEventUI != null)
            {
                yield return areaEventUI.Show(title, choices);
            }
        }

        public void CloseView()
        {
            if (areaEventUI != null)
            {
                areaEventUI.Hide();
            }
        }

        private List<AreaEventType> GetChoices()
        {
            AreaEventData data = GetCurrentOrFirstData();
            if (data != null)
            {
                return AreaEventSelectorUtil.GetNextRegionChoices(data, playerStatus);
            }

            return new List<AreaEventType> { AreaEventType.Battle, AreaEventType.Shop, AreaEventType.Blessing };
        }

        private string GetAreaTitle()
        {
            AreaEventData data = GetCurrentOrFirstData();
            if (data != null)
            {
                return data.Name;
            }
            return "다음 탐험 지역";
        }

        private AreaEventData GetCurrentOrFirstData()
        {
            if (DataCenter.areaevent_datas == null || DataCenter.areaevent_datas.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(currentAreaEventId) && DataCenter.areaevent_datas.TryGetValue(currentAreaEventId, out var data))
            {
                return data;
            }

            foreach (var kvp in DataCenter.areaevent_datas)
            {
                return kvp.Value;
            }

            return null;
        }

        private void HandleEventSelected(AreaEventType type)
        {
            UpdateStatusOnEventSelected(type);
            OnAreaEventSelected?.Invoke(type);

            switch (type)
            {
                case AreaEventType.Shop:
                    LoadingManager.Instance?.LoadScene("AreaEvent_Shop");
                    CloseView();
                    break;
                case AreaEventType.Blacksmith:
                    LoadingManager.Instance?.LoadScene("AreaEvent_Blacksmith");
                    CloseView();
                    break;
                case AreaEventType.Blessing:
                    LoadingManager.Instance?.LoadScene("AreaEvent_Blessing");
                    CloseView();
                    break;
                case AreaEventType.Battle:
                    CloseView();
                    break;
                case AreaEventType.Boss:
                    CloseView();
                    break;
                case AreaEventType.Random:
                    CloseView();
                    break;
            }
        }

        private void UpdateStatusOnEventSelected(AreaEventType type)
        {
            switch (type)
            {
                case AreaEventType.Shop:
                    playerStatus.ShopCountInStage++;
                    break;
                case AreaEventType.Blacksmith:
                    playerStatus.SmithyCountInStage++;
                    break;
                case AreaEventType.Blessing:
                    playerStatus.BlessingCooldownTurns = 3;
                    break;
            }
        }

        #region ContextMenu Test

        [ContextMenu("Test")]
        public void Test()
        {
            OpenView(currentAreaEventId);
        }

        #endregion
    }
}
