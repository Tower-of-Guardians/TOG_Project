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
        [SerializeField] private GameObject randomPrefab;

        [Header("Runtime Status")]
        [SerializeField] private string currentAreaEventId = "310102";
        [SerializeField] private PlayerEventStatus playerStatus;

        public event Action<AreaEventType> OnAreaEventSelected;
        public event Action<RandomAreaEventType> OnRandomAreaEventSelected;

        private GameObject activeAreaEventInstance;
        private Jongmin.EventDomain eventDomain;
        private BattleManager battleManager;
        private BattleManagerInjector battleManagerInjector;
        private List<AreaEventType> currentChoices;
        private bool selectionOpen;
        private bool transitioning;
        private readonly HashSet<AreaEventType> visitedEvents = new();
        private readonly AreaEventProgress progress = new();
        private bool restoredCheckpoint;
        private Jongmin.ERunCheckpointType restoredCheckpointType;
        private AreaEventType? restoredEventType;

        public bool IsRunCompleted => progress.IsCompleted;
        public string CurrentAreaEventId => currentAreaEventId;
        public RandomAreaEventType? CurrentRandomEvent { get; private set; }
        public string CurrentNpcID { get; private set; }

        public bool IsUIOpen()
        {
            if (areaEventUI != null && areaEventUI.IsUIOpen()) return true;
            return IsEventUIOpen();
        }

        public bool IsEventUIOpen()
        {
            if (activeAreaEventInstance == null) return false;
            var randomUI = activeAreaEventInstance.GetComponentInChildren<AreaEventUI_Random>(true);
            if (randomUI != null && randomUI.IsUIOpen()) return true;
            foreach (var subUI in activeAreaEventInstance.GetComponentsInChildren<AreaEventSubUI>(true))
            {
                if (subUI.IsUIOpen()) return true;
            }
            return false;
        }

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
            bool refreshChoices = !restoredCheckpoint || restoredCheckpointType != Jongmin.ERunCheckpointType.AreaSelection || currentChoices == null;
            StartCoroutine(OpenViewRoutine(refreshChoices));
        }

        public bool TryResumeSavedArea()
        {
            if (!Jongmin.RunSaveService.HasAreaCheckpoint)
            {
                return false;
            }
            if (!EnsureProgress(false))
            {
                return false;
            }

            CloseActiveEvent();
            if (restoredCheckpointType == Jongmin.ERunCheckpointType.EventEntry && restoredEventType.HasValue)
            {
                StartCoroutine(ResumeEventRoutine(restoredEventType.Value));
                return true;
            }
            bool refreshChoices = restoredCheckpointType != Jongmin.ERunCheckpointType.AreaSelection || currentChoices == null;
            StartCoroutine(OpenViewRoutine(refreshChoices));
            return true;
        }

        public bool EnsureInitialAreaCheckpoint()
        {
            if (!EnsureProgress(true))
            {
                return false;
            }
            if (progress.PendingEvent == AreaEventType.Battle)
            {
                Jongmin.RunSaveService.SaveEventEntry(currentAreaEventId, playerStatus, currentChoices, AreaEventType.Battle);
                return true;
            }

            if (!progress.TryBeginEvent(AreaEventType.Battle))
            {
                return false;
            }

            Jongmin.RunSaveService.SaveEventEntry(currentAreaEventId, playerStatus, currentChoices, AreaEventType.Battle);
            return true;
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
            if (!restoredCheckpoint && Jongmin.RunSaveService.TryGetAreaResume(out string savedAreaID, out PlayerEventStatus savedStatus, out Jongmin.ERunCheckpointType checkpointType, out List<AreaEventType> savedChoices, out AreaEventType? savedEventType))
            {
                initialId = savedAreaID;
                playerStatus = savedStatus;
                if ((checkpointType == Jongmin.ERunCheckpointType.AreaSelection || checkpointType == Jongmin.ERunCheckpointType.EventEntry) && savedChoices is { Count: > 0 })
                {
                    currentChoices = savedChoices;
                }
                restoredCheckpointType = checkpointType;
                restoredEventType = savedEventType;
                restoredCheckpoint = true;
            }
            if (!progress.TryInitialize(DataCenter.areaevent_datas?.Values, initialId))
            {
                Debug.LogError($"[AreaEventDomain] 지역 진행 데이터를 찾을 수 없습니다: {initialId}", this);
                return false;
            }

            currentAreaEventId = progress.Current.Id;
            if (previousStage >= 0 && previousStage != progress.Current.Stage) ResetStageCounts();
            RecordReachedStage();
            if (currentChoices == null && !restoredCheckpoint)
            {
                Jongmin.RunSaveService.SaveAreaEntry(currentAreaEventId, playerStatus);
            }
            return true;
        }

        private void RecordReachedStage()
        {
            Jongmin.EventDomain cachedEventDomain = GetEventDomain();
            if (progress.Current != null && cachedEventDomain != null)
            {
                cachedEventDomain.RecordReachedStage(progress.Current.Stage);
            }
        }

        private void CompleteAreaEvent(AreaEventType type, bool succeeded)
        {
            int previousStage = progress.Current.Stage;
            if (!progress.TryCompleteEvent(type, succeeded)) return;

            if (succeeded && type == AreaEventType.Boss)
            {
                GetEventDomain()?.RecordRunResult(Jongmin.ERunEndReason.BossDefeated);
            }

            CloseActiveEvent();
            if (!succeeded)
            {
                Jongmin.RunSaveService.EndRun(Jongmin.ERunEndReason.Death);
                transitioning = true;
                LoadingManager.Instance.LoadScene("Lobby");
                return;
            }

            currentAreaEventId = progress.Current.Id;
            if (progress.IsCompleted)
            {
                Jongmin.RunSaveService.EndRun(Jongmin.ERunEndReason.Clear);
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
                Jongmin.RunSaveService.SaveAreaSelection(currentAreaEventId, playerStatus, currentChoices);
            }

            if (areaEventUI != null)
            {
                yield return areaEventUI.Show(GetAreaTitle(), currentChoices);
                foreach (AreaEventType type in currentChoices)
                {
                    bool implemented = type != AreaEventType.Blessing;
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

            if (type == AreaEventType.Blessing)
            {
                Debug.LogWarning("[AreaEventDomain] 축복 보상 처리가 아직 구현되지 않았습니다.", this);
                return;
            }

            Jongmin.RunSaveService.SaveEventEntry(currentAreaEventId, playerStatus, currentChoices, type);
            StartSelectedEvent(type);
        }

        private void StartSelectedEvent(AreaEventType type)
        {
            switch (type)
            {
                case AreaEventType.Shop:
                    OpenAreaEventPrefab(shopPrefab, type);
                    break;
                case AreaEventType.Blacksmith:
                    OpenAreaEventPrefab(blacksmithPrefab, type);
                    break;
                case AreaEventType.Battle:
                case AreaEventType.Boss:
                    StartAreaBattle(type);
                    break;
                case AreaEventType.Random:
                    OpenAreaEventPrefab(randomPrefab, type);
                    break;
            }
        }

        private IEnumerator ResumeEventRoutine(AreaEventType type)
        {
            transitioning = true;
            selectionOpen = false;
            yield return null;
            transitioning = false;
            StartSelectedEvent(type);
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
            EndNpcEncounter();
            CurrentRandomEvent = null;
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
            if (prefab == null || (type == AreaEventType.Random
                    ? prefab.GetComponentInChildren<AreaEventUI_Random>(true) == null
                    : prefab.GetComponentInChildren<AreaEventSubUI>(true) == null))
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
                    subUI.Bind(CompleteActiveEvent, npcRoots);
                }

                UpdateStatusOnEventSelected(type);
                visitedEvents.Add(type);
                progress.TryBeginEvent(type);
                if (type == AreaEventType.Random)
                {
                    CurrentRandomEvent = RandomAreaEventSelector.Select();
                    activeAreaEventInstance.GetComponentInChildren<AreaEventUI_Random>(true)
                        .Show(CurrentRandomEvent.Value, CompleteActiveEvent);
                }
                BeginNpcEncounter(type);
                transitioning = false;
                OnAreaEventSelected?.Invoke(type);
                if (CurrentRandomEvent.HasValue)
                {
                    OnRandomAreaEventSelected?.Invoke(CurrentRandomEvent.Value);
                }
            });
        }

        public void CompleteActiveEvent()
        {
            if (transitioning || activeAreaEventInstance == null) return;
            AreaEventType? type = progress.PendingEvent;
            if (type != AreaEventType.Random && IsEventUIOpen()) return;
            if (type != AreaEventType.Shop && type != AreaEventType.Blacksmith && type != AreaEventType.Random) return;
            if (type == AreaEventType.Random && CurrentRandomEvent.HasValue)
            {
                string eventID = CurrentRandomEvent.Value.ToString();
                Jongmin.RunSaveService.MarkRandomEventExperienced(eventID);
                GetEventDomain()?.RecordEventExperience(eventID);
            }
            CompleteAreaEvent(type.Value, true);
        }

        private string GetNpcID(AreaEventType type)
        {
            return type switch
            {
                AreaEventType.Shop => "Krabian",
                AreaEventType.Blacksmith => "Heca",
                AreaEventType.Random => CurrentRandomEvent switch
                {
                    RandomAreaEventType.Vampire => "Lucien",
                    RandomAreaEventType.Spirit => "Elemental",
                    RandomAreaEventType.Cleric => "EventPriest",
                    RandomAreaEventType.Jester => "Clown",
                    RandomAreaEventType.TrainingDummy => "TrainingDoll",
                    _ => null
                },
                _ => null
            };
        }

        private void BeginNpcEncounter(AreaEventType type)
        {
            string npcID = GetNpcID(type);
            if (string.IsNullOrEmpty(npcID) || CurrentNpcID == npcID) return;

            EndNpcEncounter();
            Jongmin.EventDomain cachedEventDomain = GetEventDomain();
            if (cachedEventDomain == null)
            {
                Debug.LogWarning($"[AreaEventDomain] NPC 조우를 기록할 EventDomain이 없습니다: {npcID}", this);
                return;
            }

            cachedEventDomain.RecordNpcEncounter(npcID);
            CurrentNpcID = npcID;
        }

        private void EndNpcEncounter()
        {
            if (eventDomain != null && !string.IsNullOrEmpty(CurrentNpcID))
            {
                eventDomain.EndNpcEncounter(CurrentNpcID);
            }
            CurrentNpcID = null;
        }

        private void StartAreaBattle(AreaEventType type)
        {
            if (GetBattleManager() == null)
            {
                Debug.LogError("[AreaEventDomain] BattleManager가 등록되어 있지 않습니다.", this);
                return;
            }

            BattleManagerInjector injector = GetBattleManagerInjector();
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

        private Jongmin.EventDomain GetEventDomain()
        {
            if (eventDomain != null)
            {
                return eventDomain;
            }
            if (!DIContainer.IsRegistered<Jongmin.EventDomain>())
            {
                return null;
            }

            eventDomain = DIContainer.Resolve<Jongmin.EventDomain>();
            return eventDomain;
        }

        private BattleManager GetBattleManager()
        {
            if (battleManager != null)
            {
                return battleManager;
            }
            if (!DIContainer.IsRegistered<BattleManager>())
            {
                return null;
            }

            battleManager = DIContainer.Resolve<BattleManager>();
            return battleManager;
        }

        private BattleManagerInjector GetBattleManagerInjector()
        {
            if (battleManagerInjector != null)
            {
                return battleManagerInjector;
            }
            if (!DIContainer.IsRegistered<BattleManagerInjector>())
            {
                return null;
            }

            battleManagerInjector = DIContainer.Resolve<BattleManagerInjector>();
            return battleManagerInjector;
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
