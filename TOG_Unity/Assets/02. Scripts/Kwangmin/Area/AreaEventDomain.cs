using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kwangmin
{
    public class AreaEventDomain : MonoBehaviour
    {
        [SerializeField] private AreaEventUI areaEventUI;

        [Header("Area Event Prefabs")]
        [SerializeField] private GameObject shopPrefab;
        [SerializeField] private GameObject blacksmithPrefab;
        [SerializeField] private GameObject blessingPrefab;

        [Header("Runtime Status")]
        [SerializeField] private string currentAreaEventId = "310102";
        [SerializeField] private PlayerEventStatus playerStatus;

        public event Action<AreaEventType> OnAreaEventSelected;

        private GameObject activeAreaEventInstance;
        private List<AreaEventType> currentChoices;
        private bool selectionOpen;
        private bool transitioning;
        private readonly HashSet<AreaEventType> visitedEvents = new();
        private readonly AreaEventProgress progress = new();

        public bool IsRunCompleted => progress.IsCompleted;
        public string CurrentAreaEventId => currentAreaEventId;

        private void Awake()
        {
            if (areaEventUI != null)
            {
                areaEventUI.Bind(HandleEventSelected);
            }
        }

        public void OpenView(string areaEventId = null)
        {
            if (transitioning || progress.IsCompleted) return;
            if (!EnsureProgress(false, areaEventId)) return;
            CloseActiveEvent();
            StartCoroutine(OpenViewRoutine(true));
        }

        public void CompleteBattleResult(bool isVictory)
        {
            if (transitioning || progress.IsCompleted) return;
            if (!progress.IsInitialized)
            {
                if (!EnsureProgress(true)) return;
                progress.TryBeginEvent(AreaEventType.Battle);
            }

            AreaEventType? type = progress.PendingEvent;
            if (type != AreaEventType.Battle && type != AreaEventType.Boss) return;
            CompleteAreaEvent(type.Value, isVictory);
        }

        private bool EnsureProgress(bool firstBattle, string areaEventId = null)
        {
            if (progress.IsInitialized && string.IsNullOrEmpty(areaEventId)) return true;

            int previousStage = progress.Current != null ? progress.Current.Stage : -1;
            string initialId = firstBattle ? null : (string.IsNullOrEmpty(areaEventId) ? currentAreaEventId : areaEventId);
            if (!progress.TryInitialize(DataCenter.areaevent_datas?.Values, initialId))
            {
                Debug.LogError($"[AreaEventDomain] 지역 진행 데이터를 찾을 수 없습니다: {initialId}", this);
                return false;
            }

            currentAreaEventId = progress.Current.Id;
            if (previousStage >= 0 && previousStage != progress.Current.Stage) ResetStageCounts();
            RecordReachedStage();
            return true;
        }

        private void RecordReachedStage()
        {
            if (progress.Current != null && DIContainer.IsRegistered<Jongmin.EventDomain>())
            {
                DIContainer.Resolve<Jongmin.EventDomain>()?.RecordReachedStage(progress.Current.Stage);
            }
        }

        private void CompleteAreaEvent(AreaEventType type, bool succeeded)
        {
            int previousStage = progress.Current.Stage;
            if (!progress.TryCompleteEvent(type, succeeded)) return;

            CloseActiveEvent();
            if (!succeeded)
            {
                StartCoroutine(OpenViewRoutine(false));
                return;
            }

            currentAreaEventId = progress.Current.Id;
            if (progress.IsCompleted)
            {
                selectionOpen = false;
                transitioning = true;
                areaEventUI?.Hide();
                LoadingManager.Instance.LoadScene("Lobby");
                return;
            }

            if (previousStage != progress.Current.Stage)
            {
                ResetStageCounts();
                RecordReachedStage();
            }

            playerStatus.DecreaseBlessingCooldown();
            StartCoroutine(OpenViewRoutine(true));
        }

        public void ResetStageCounts()
        {
            playerStatus.ResetStageCounts();
        }

        private IEnumerator OpenViewRoutine(bool refreshChoices)
        {
            transitioning = true;
            selectionOpen = false;
            if (refreshChoices || currentChoices == null)
            {
                currentChoices = GetChoices();
                visitedEvents.Clear();
            }

            if (areaEventUI != null)
            {
                yield return areaEventUI.Show(GetAreaTitle(), currentChoices);
                foreach (AreaEventType type in currentChoices)
                {
                    bool implemented = type != AreaEventType.Random && type != AreaEventType.Blessing;
                    areaEventUI.SetEventAvailable(type, implemented && !visitedEvents.Contains(type));
                }
            }
            selectionOpen = true;
            transitioning = false;
        }

        public void CloseView()
        {
            selectionOpen = false;
            if (areaEventUI != null)
            {
                areaEventUI.Hide();
                return;
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
            if (!selectionOpen || transitioning || progress.IsCompleted || progress.PendingEvent.HasValue
                || visitedEvents.Contains(type)) return;

            switch (type)
            {
                case AreaEventType.Shop:
                    OpenAreaEventPrefab(shopPrefab, type);
                    break;
                case AreaEventType.Blacksmith:
                    OpenAreaEventPrefab(blacksmithPrefab, type);
                    break;
                case AreaEventType.Blessing:
                    Debug.LogWarning("[AreaEventDomain] 축복 보상 처리가 아직 구현되지 않았습니다.", this);
                    break;
                case AreaEventType.Battle:
                case AreaEventType.Boss:
                    StartAreaBattle(type);
                    break;
                case AreaEventType.Random:
                    Debug.LogWarning("[AreaEventDomain] 랜덤 이벤트 처리가 아직 구현되지 않았습니다.", this);
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

        public void CloseActiveEvent()
        {
            if (activeAreaEventInstance == null)
            {
                return;
            }

            foreach (var subUI in activeAreaEventInstance.GetComponentsInChildren<AreaEventSubUI>(true))
            {
                subUI.Bind(null, null);
            }
            activeAreaEventInstance.SetActive(false);
            Destroy(activeAreaEventInstance);
            activeAreaEventInstance = null;
        }

        private void OpenAreaEventPrefab(GameObject prefab, AreaEventType type)
        {
            if (prefab == null || prefab.GetComponentInChildren<AreaEventSubUI>(true) == null)
            {
                Debug.LogError($"[AreaEventDomain] {type} 프리팹 또는 하위 UI가 할당되어 있지 않습니다.", this);
                return;
            }

            transitioning = true;
            selectionOpen = false;
            areaEventUI.Hide(() =>
            {
                CloseActiveEvent();
                AreaSceneBootstrapper.DisableGameBattleObjects(this);

                activeAreaEventInstance = Instantiate(prefab);
                activeAreaEventInstance.name = prefab.name;

                Scene ownerScene = gameObject.scene;
                if (ownerScene.IsValid() && ownerScene.isLoaded && activeAreaEventInstance.scene != ownerScene)
                {
                    SceneManager.MoveGameObjectToScene(activeAreaEventInstance, ownerScene);
                }

                var npcs = activeAreaEventInstance.GetComponentsInChildren<ClickableObject>(true);
                var npcRoots = new GameObject[npcs.Length];
                for (int i = 0; i < npcs.Length; i++) npcRoots[i] = npcs[i].gameObject;
                foreach (var subUI in activeAreaEventInstance.GetComponentsInChildren<AreaEventSubUI>(true))
                {
                    subUI.Bind(ReturnFromAreaEvent, npcRoots);
                }

                UpdateStatusOnEventSelected(type);
                visitedEvents.Add(type);
                progress.TryBeginEvent(type);
                transitioning = false;
                OnAreaEventSelected?.Invoke(type);
            });
        }

        private void ReturnFromAreaEvent()
        {
            if (transitioning) return;
            AreaEventType? type = progress.PendingEvent;
            if (type != AreaEventType.Shop && type != AreaEventType.Blacksmith) return;
            CompleteAreaEvent(type.Value, true);
        }

        private void StartAreaBattle(AreaEventType type)
        {
            if (!DIContainer.IsRegistered<BattleManager>())
            {
                Debug.LogError("[AreaEventDomain] BattleManager가 등록되어 있지 않습니다.", this);
                return;
            }

            var injector = DIContainer.IsRegistered<BattleManagerInjector>()
                ? DIContainer.Resolve<BattleManagerInjector>() : null;
            if (injector == null)
            {
                Debug.LogError("[AreaEventDomain] BattleManagerInjector가 없습니다.", this);
                return;
            }

            transitioning = true;
            selectionOpen = false;
            areaEventUI.Hide(() =>
            {
                bool accepted = injector.TryStartAreaBattle(GetCurrentOrFirstData(), type, success =>
                {
                    transitioning = false;
                    if (success)
                    {
                        progress.TryBeginEvent(type);
                        OnAreaEventSelected?.Invoke(type);
                    }
                    else
                    {
                        AreaSceneBootstrapper.DisableGameBattleObjects(this);
                        StartCoroutine(OpenViewRoutine(false));
                    }
                });
                if (accepted)
                {
                    CloseActiveEvent();
                    AreaSceneBootstrapper.EnableGameBattleObjects(this);
                }
                else
                {
                    transitioning = false;
                    StartCoroutine(OpenViewRoutine(false));
                }
            });
        }

        private void OnDestroy()
        {
            CloseActiveEvent();
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
