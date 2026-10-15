using System;
using System.Collections.Generic;
using JxModule;
using UnityEngine;

namespace Jongmin
{
    public static class RunSaveService
    {
        private static TogSaveData _data;
        private static EventDomain _eventDomain;
        private static RelicDomain _relicDomain;
        private static string _restoredRunID;
        private static string _restoredRandomRunID;
        private static readonly SlotPlayClock PlayClock = new();
        private static SlotSessionLifetime _sessionLifetime;
        private static bool _selectedSlotCleared;

        public static int SelectedSlot { get; private set; }
        public static bool HasActiveRun => Data.activeRun != null;
        public static bool HasAreaCheckpoint => HasActiveRun && !string.IsNullOrWhiteSpace(Data.activeRun.areaID);
        public static ActiveRunSaveData ActiveRun => Data.activeRun;

        private static TogSaveData Data
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            _data = null;
            _eventDomain = null;
            _relicDomain = null;
            _restoredRunID = null;
            _restoredRandomRunID = null;
            PlayClock.Stop();
            _sessionLifetime = null;
            SelectedSlot = 0;
            _selectedSlotCleared = false;
            EnsureLoaded();
            RestoreRandomState();
        }

        public static void SelectSlot(int slot)
        {
            if (slot < 0 || slot >= SaveSlotSystem.MaxSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }

            EndSession();
            SelectedSlot = slot;
            _data = SlotSaveDataReader.TryLoad(slot, out var loaded) ? loaded : new TogSaveData();
            Normalize();
            _restoredRunID = null;
            _restoredRandomRunID = null;
            _eventDomain?.ResetRunProgress();
            DataCenter.Instance?.ResetForNewRun();
            RestoreRandomState();
            RestoreBoundDomains();
            
            if (DataCenter.IsCardDataLoaded && (_data.activeRun == null || !_data.activeRun.hasBattleDeckState))
            {
                GameData.Instance?.ResetBattleDeck();
            }
            
            StartSession();
        }

        public static void Bind(EventDomain eventDomain, RelicDomain relicDomain)
        {
            _eventDomain = eventDomain;
            _relicDomain = relicDomain;
            RestoreBoundDomains();
        }

        public static void ClearSlot(int slot)
        {
            if (slot < 0 || slot >= SaveSlotSystem.MaxSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }

            SaveSlotSystem.Remove(slot);
            if (SelectedSlot != slot)
            {
                return;
            }

            _selectedSlotCleared = true;
            PlayClock.Stop();
            _data = new TogSaveData();
            _data.slotInfo.slot = slot;
            _restoredRunID = null;
            _restoredRandomRunID = null;
            _eventDomain?.ResetRunProgress();
            RestoreRandomState();
            RestoreBoundDomains();
            
            if (Application.isPlaying)
            {
                DataCenter.Instance?.ResetForNewRun();
                if (DataCenter.IsCardDataLoaded)
                {
                    GameData.Instance?.ResetBattleDeck();
                }
            }
        }

        public static void StartSession()
        {
            if (PlayClock.IsRunning)
            {
                return;
            }
            
            EnsureLoaded();
            PlayClock.Start(Time.realtimeSinceStartupAsDouble);
            _selectedSlotCleared = false;
            Save();
            
            if (Application.isPlaying && _sessionLifetime == null)
            {
                var owner = new GameObject("Slot Session");
                UnityEngine.Object.DontDestroyOnLoad(owner);
                _sessionLifetime = owner.AddComponent<SlotSessionLifetime>();
            }
        }

        public static void SaveSession()
        {
            if (PlayClock.IsRunning)
            {
                Save();
            }
        }

        public static void EndSession()
        {
            SaveSession();
            PlayClock.Stop();
        }

        public static void BeginRun(int? seed = null)
        {
            EnsureLoaded();
            DataCenter.Instance?.ResetForNewRun();
            
            var runSeed = seed ?? unchecked((int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            _data.activeRun = new ActiveRunSaveData
            {
                runID = Guid.NewGuid().ToString("N"),
                seed = runSeed,
                checkpointType = ERunCheckpointType.AreaEntry
            };
            
            _eventDomain?.ResetRunProgress();
            RunRandom.Initialize(runSeed);
            _restoredRandomRunID = _data.activeRun.runID;
            
            if (DataCenter.IsCardDataLoaded)
            {
                GameData.Instance?.ResetBattleDeck();
            }
            
            CaptureRuntime(_data.activeRun);
            _data.runCount++;
            Save();
            _restoredRunID = _data.activeRun.runID;
        }

        public static void SaveAreaEntry(string areaID, PlayerEventStatus status)
        {
            EnsureRun();
            
            _data.activeRun.checkpointType = ERunCheckpointType.AreaEntry;
            _data.activeRun.areaID = areaID;
            _data.activeRun.eventStatus = status;
            _data.activeRun.areaChoices.Clear();
            
            CaptureRuntime(_data.activeRun);
            Save();
        }

        public static void SaveAreaSelection(string areaID, PlayerEventStatus status, IEnumerable<AreaEventType> choices)
        {
            EnsureRun();
            
            _data.activeRun.checkpointType = ERunCheckpointType.AreaSelection;
            _data.activeRun.areaID = areaID;
            _data.activeRun.eventStatus = status;
            _data.activeRun.areaChoices.Clear();
            
            if (choices != null)
            {
                foreach (var choice in choices)
                {
                    _data.activeRun.areaChoices.Add((int)choice);
                }
            }
            
            CaptureRuntime(_data.activeRun);
            Save();
        }

        public static void SaveEventEntry(string areaID, PlayerEventStatus status, IEnumerable<AreaEventType> choices, AreaEventType eventType)
        {
            EnsureRun();
            
            _data.activeRun.checkpointType = ERunCheckpointType.EventEntry;
            _data.activeRun.areaID = areaID;
            _data.activeRun.eventStatus = status;
            _data.activeRun.eventType = (int)eventType;
            _data.activeRun.areaChoices.Clear();
            
            if (choices != null)
            {
                foreach (var choice in choices)
                {
                    _data.activeRun.areaChoices.Add((int)choice);
                }
            }
            
            CaptureRuntime(_data.activeRun);
            Save();
        }

        public static bool TryGetAreaResume(out string areaID, out PlayerEventStatus status, out ERunCheckpointType checkpointType, out List<AreaEventType> choices, out AreaEventType? eventType)
        {
            RestoreBoundDomains();
            
            var run = Data.activeRun;
            if (run == null || string.IsNullOrWhiteSpace(run.areaID))
            {
                areaID = null;
                status = default;
                checkpointType = default;
                choices = null;
                eventType = null;
                return false;
            }

            areaID = run.areaID;
            status = run.eventStatus;
            checkpointType = run.checkpointType;
            choices = new List<AreaEventType>();
            
            if (run.areaChoices != null)
            {
                foreach (var choice in run.areaChoices)
                {
                    choices.Add((AreaEventType)choice);
                }
            }
            
            eventType = run.checkpointType == ERunCheckpointType.EventEntry ? (AreaEventType)run.eventType : null;
            
            return true;
        }

        public static void EndRun(ERunEndReason reason)
        {
            if (!HasActiveRun)
            {
                return;
            }
            
            _eventDomain?.RecordRunResult(reason);
            _data.slotProgress = CaptureSlotProgress();
            _data.activeRun = null;
            _restoredRunID = null;
            _restoredRandomRunID = null;
            RunRandom.Initialize(0);
            Save();
            _eventDomain?.ResetRunProgress();
        }

        public static void AbandonRunAndReturnToLobby()
        {
            EndRun(ERunEndReason.Abandon);
            
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadScene("Lobby");
            }
        }

        public static void MarkRandomEventExperienced(string eventID)
        {
            if (string.IsNullOrWhiteSpace(eventID))
            {
                return;
            }
            
            EnsureRun();
            
            if (!_data.activeRun.experiencedRandomEventIDs.Contains(eventID))
            {
                _data.activeRun.experiencedRandomEventIDs.Add(eventID);
            }
        }

        public static bool HasExperiencedRandomEvent(string eventID)
        {
            return HasActiveRun && Data.activeRun.experiencedRandomEventIDs.Contains(eventID);
        }

        public static void SaveSlotProgress()
        {
            EnsureLoaded();
            _data.slotProgress = CaptureSlotProgress();
            Save();
        }

        public static void RestoreAfterCardDataLoad()
        {
            RestoreBoundDomains();
        }

        public static bool TryRestoreBattleDeckState()
        {
            var run = Data.activeRun;
            if (run == null || !run.hasBattleDeckState || GameData.Instance == null)
            {
                return false;
            }

            var gameData = GameData.Instance;
            gameData.notuseDeck.Clear();
            gameData.notuseDeck.AddRange(run.drawDeck);
            gameData.handDeck.Clear();
            gameData.handDeck.AddRange(run.handDeck);
            gameData.garbageDeck.Clear();
            gameData.garbageDeck.AddRange(run.discardDeck);
            gameData.attackField.Clear();
            gameData.defenseField.Clear();
            return true;
        }

        private static void EnsureRun()
        {
            if (!HasActiveRun)
            {
                BeginRun();
            }
        }

        private static void CaptureRuntime(ActiveRunSaveData run)
        {
            if (run == null)
            {
                return;
            }
            
            _data.slotProgress = CaptureSlotProgress();
            run.experienceRecords = _eventDomain?.CaptureRunProgress() ?? new List<ExperienceRecordData>();
            run.randomStreams = RunRandom.Capture();
            run.deck = CaptureDeck();
            CaptureBattleDeckState(run);
            run.player = CapturePlayer();
        }

        private static void CaptureBattleDeckState(ActiveRunSaveData run)
        {
            var gameData = GameData.Instance;
            if (gameData == null || gameData.notuseDeck.Count + gameData.handDeck.Count + gameData.garbageDeck.Count == 0)
            {
                run.hasBattleDeckState = false;
                run.drawDeck.Clear();
                run.handDeck.Clear();
                run.discardDeck.Clear();
                return;
            }

            run.hasBattleDeckState = true;
            run.drawDeck = new List<string>(gameData.notuseDeck);
            run.handDeck = new List<string>(gameData.handDeck);
            run.discardDeck = new List<string>(gameData.garbageDeck);
        }

        private static SlotProgressSaveData CaptureSlotProgress()
        {
            if (_eventDomain != null)
            {
                return _eventDomain.CaptureSlotProgress();
            }
            
            var slotProgress = _data?.slotProgress ?? new SlotProgressSaveData();
            if (_relicDomain != null)
            {
                slotProgress.relicIDs = new List<string>(_relicDomain.OwnedRelicIDs);
            }
            
            return slotProgress;
        }

        private static List<CardRunSaveData> CaptureDeck()
        {
            var result = new List<CardRunSaveData>();
            if (DataCenter.Instance?.userDeck == null)
            {
                return result;
            }
            
            foreach (var card in DataCenter.Instance.userDeck)
            {
                if (card == null || string.IsNullOrWhiteSpace(card.id))
                {
                    continue;
                }
                
                result.Add(new CardRunSaveData
                {
                    cardID = card.id,
                    grade = card.grade,
                    star = card.star,
                    attack = card.ATK,
                    defense = card.DEF,
                    effect1Value = card.effect1Value,
                    effect2Value = card.effect2Value,
                    acquiredAt = card.time
                });
            }
            return result;
        }

        private static PlayerRunSaveData CapturePlayer()
        {
            var state = DataCenter.Instance?.playerstate;
            if (state == null)
            {
                return new PlayerRunSaveData();
            }
            
            return new PlayerRunSaveData
            {
                level = state.level,
                maxExperience = state.maxexperience,
                experience = state.experience,
                hp = state.hp,
                maxHp = state.maxhp,
                attack = state.atk,
                maxMagic = state.maxmagic,
                gold = state.money
            };
        }

        private static void RestoreBoundDomains()
        {
            EnsureLoaded();
            _eventDomain?.RestoreProgress(_data.slotProgress, _data.activeRun?.experienceRecords);
            _relicDomain?.RestoreOwnedRelics(_data.slotProgress?.relicIDs);

            if (_data.activeRun == null || _restoredRunID == _data.activeRun.runID)
            {
                return;
            }
            
            RestoreRandomState();
            RestorePlayer(_data.activeRun.player);
            
            if (RestoreDeck(_data.activeRun.deck))
            {
                TryRestoreBattleDeckState();
                _restoredRunID = _data.activeRun.runID;
            }
        }

        private static void RestoreRandomState()
        {
            var run = _data?.activeRun;
            if (run == null)
            {
                RunRandom.Initialize(0);
                _restoredRandomRunID = null;
                return;
            }
            
            if (_restoredRandomRunID == run.runID)
            {
                return;
            }

            RunRandom.Initialize(run.seed, run.randomStreams);
            _restoredRandomRunID = run.runID;
        }

        private static void RestorePlayer(PlayerRunSaveData saved)
        {
            if (saved == null || DataCenter.Instance?.playerstate == null)
            {
                return;
            }
            
            var state = DataCenter.Instance.playerstate;
            state.level = saved.level;
            state.maxexperience = saved.maxExperience;
            state.experience = saved.experience;
            state.hp = saved.hp;
            state.maxhp = saved.maxHp;
            state.atk = saved.attack;
            state.maxmagic = saved.maxMagic;
            state.money = saved.gold;
            DataCenter.Instance.SetPlayerState();
        }

        private static bool RestoreDeck(IEnumerable<CardRunSaveData> savedDeck)
        {
            if (savedDeck == null || DataCenter.Instance == null || !DataCenter.IsCardDataLoaded)
            {
                return false;
            }
            
            DataCenter.Instance.userDeck.Clear();
            foreach (var saved in savedDeck)
            {
                if (saved == null || !DataCenter.card_datas.TryGetValue(saved.cardID, out var source))
                {
                    continue;
                }
                var card = UnityEngine.Object.Instantiate(source);
                card.grade = saved.grade;
                card.star = saved.star;
                card.ATK = saved.attack;
                card.DEF = saved.defense;
                card.effect1Value = saved.effect1Value;
                card.effect2Value = saved.effect2Value;
                card.time = saved.acquiredAt;
                DataCenter.Instance.userDeck.Add(card);
            }
            
            return true;
        }

        private static void Save()
        {
            if (_selectedSlotCleared)
            {
                return;
            }
            _data.slotInfo.playTime += PlayClock.CollectSeconds(Time.realtimeSinceStartupAsDouble);
            SaveSlotSystem.Save(SelectedSlot, _data, true);
        }

        private static void EnsureLoaded()
        {
            if (_data != null)
            {
                return;
            }
            
            _data = SlotSaveDataReader.TryLoad(SelectedSlot, out var loaded) ? loaded : new TogSaveData();
            Normalize();
        }

        private static void Normalize()
        {
            _data ??= new TogSaveData();
            _data.slotInfo ??= new SaveSlotInfo();
            _data.slotProgress ??= new SlotProgressSaveData();
            
            if (_data.activeRun != null)
            {
                _data.activeRun.deck ??= new List<CardRunSaveData>();
                _data.activeRun.drawDeck ??= new List<string>();
                _data.activeRun.handDeck ??= new List<string>();
                _data.activeRun.discardDeck ??= new List<string>();
                _data.activeRun.player ??= new PlayerRunSaveData();
                _data.activeRun.experienceRecords ??= new List<ExperienceRecordData>();
                _data.activeRun.areaChoices ??= new List<int>();
                _data.activeRun.randomStreams ??= new List<RandomStreamSaveData>();
                _data.activeRun.experiencedRandomEventIDs ??= new List<string>();
            }
        }
    }
}
