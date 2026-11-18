using System.Collections.Generic;
using System.Linq;
using JxModule.Terminal;
using Jongmin;
using NUnit.Framework;
using UnityEngine;

public sealed class RunProgressTests
{
    [Test]
    public void RunRandom_RestoredStreamContinuesWithSameResult()
    {
        RunRandom.Initialize(12345);
        RunRandom.Range(RunRandom.Reward, 0, 1000);
        var snapshot = RunRandom.Capture();
        var expectedNext = RunRandom.Range(RunRandom.Reward, 0, 1000);

        RunRandom.Initialize(12345, snapshot);
        var restoredNext = RunRandom.Range(RunRandom.Reward, 0, 1000);

        Assert.AreEqual(expectedNext, restoredNext);
    }

    [Test]
    public void RunRandom_UsingOneStreamDoesNotChangeAnotherStream()
    {
        RunRandom.Initialize(777);
        var expectedArea = RunRandom.Range(RunRandom.Area, 0, 1000);

        RunRandom.Initialize(777);
        RunRandom.Range(RunRandom.EnemyAction, 0, 1000);
        RunRandom.Range(RunRandom.EnemyAction, 0, 1000);
        var actualArea = RunRandom.Range(RunRandom.Area, 0, 1000);

        Assert.AreEqual(expectedArea, actualArea);
    }

    [Test]
    public void ExperienceRecordStore_CaptureAndRestorePreservesCountsAndMaximums()
    {
        var source = new ExperienceRecordStore();
        source.Add(EExperienceRecordType.MonsterKill, "Slime", amount: 3);
        source.Max(EExperienceRecordType.HighestValue, "SingleAttackDamage", 20);
        source.Max(EExperienceRecordType.HighestValue, "SingleAttackDamage", 10);

        var restored = new ExperienceRecordStore();
        restored.Restore(source.Capture());

        Assert.AreEqual(3, restored.Get(EExperienceRecordType.MonsterKill, "Slime"));
        Assert.AreEqual(20, restored.Get(EExperienceRecordType.HighestValue, "SingleAttackDamage"));
    }

    [Test]
    public void TogSaveData_JsonRoundTripPreservesSlotProgressAndRunData()
    {
        var source = new TogSaveData
        {
            slotProgress = new SlotProgressSaveData
            {
                relicIDs = new List<string> { "RelicA" },
                seenEventIDs = new List<string> { "EventA" }
            },
            activeRun = new ActiveRunSaveData
            {
                runID = "run-1",
                seed = 42,
                areaID = "310102",
                checkpointType = ERunCheckpointType.EventEntry,
                areaChoices = new List<int> { 0, 1, 2 },
                eventType = (int)AreaEventType.Shop,
                hasBattleDeckState = true,
                drawDeck = new List<string> { "CardB", "CardA" },
                handDeck = new List<string> { "CardC" },
                discardDeck = new List<string> { "CardD" }
            }
        };
        source.slotInfo.slot = 2;

        var json = JsonUtility.ToJson(source);
        var restored = JsonUtility.FromJson<TogSaveData>(json);

        Assert.AreEqual(2, restored.slotInfo.slot);
        Assert.AreEqual("RelicA", restored.slotProgress.relicIDs[0]);
        Assert.AreEqual("EventA", restored.slotProgress.seenEventIDs[0]);
        Assert.AreEqual("run-1", restored.activeRun.runID);
        Assert.AreEqual(ERunCheckpointType.EventEntry, restored.activeRun.checkpointType);
        Assert.AreEqual(3, restored.activeRun.areaChoices.Count);
        Assert.AreEqual(AreaEventType.Shop, (AreaEventType)restored.activeRun.eventType);
        CollectionAssert.AreEqual(new[] { "CardB", "CardA" }, restored.activeRun.drawDeck);
        CollectionAssert.AreEqual(new[] { "CardC" }, restored.activeRun.handDeck);
        CollectionAssert.AreEqual(new[] { "CardD" }, restored.activeRun.discardDeck);
    }

    [Test]
    public void TogSaveData_SlotProgressJsonRestoresRecords()
    {
        const string json = "{\"version\":1,\"slotProgress\":{\"relicIDs\":[\"RelicA\"],\"seenEventIDs\":[\"EventA\"],\"experienceRecords\":[{\"type\":0,\"targetID\":\"Slime\",\"value\":3}]}}";
        var saved = JsonUtility.FromJson<TogSaveData>(json);
        var progress = new SlotEventProgress();
        progress.Restore(saved.slotProgress);

        Assert.IsTrue(progress.HasSeen("EventA"));
        Assert.AreEqual(3, progress.Experience.Get(EExperienceRecordType.MonsterKill, "Slime"));
        CollectionAssert.AreEqual(new[] { "RelicA" }, saved.slotProgress.relicIDs);
        Assert.AreEqual(1, (int)EExperienceRecordScope.Slot);
    }

    [Test]
    public void SlotProgress_RestoreReplacesPreviousSlotAndKeepsSnapshotsIndependent()
    {
        var progress = new SlotEventProgress();
        progress.MarkSeen("EventA");
        progress.AddExperience(EExperienceRecordType.MonsterKill, "Slime", amount: 3);
        progress.RecordSingleAttackDamage(20);
        var firstSlot = progress.Capture(new[] { "RelicA" });

        progress.Restore(new SlotProgressSaveData());
        Assert.IsFalse(progress.HasSeen("EventA"));
        Assert.AreEqual(0, progress.Experience.Get(EExperienceRecordType.MonsterKill, "Slime"));
        Assert.AreEqual(0, progress.MaxSingleAttackDamage);
        progress.MarkSeen("EventB");

        progress.Restore(firstSlot);
        Assert.IsTrue(progress.HasSeen("EventA"));
        Assert.IsFalse(progress.HasSeen("EventB"));
        Assert.AreEqual(3, progress.Experience.Get(EExperienceRecordType.MonsterKill, "Slime"));
        Assert.AreEqual(20, progress.MaxSingleAttackDamage);
    }

    [Test]
    public void EventProgressCache_ResetRunKeepsSlotProgress()
    {
        var cache = new EventProgressCache();
        cache.Slot.MarkSeen("EventA");
        cache.Slot.AddExperience(EExperienceRecordType.MonsterKill, "Slime", amount: 3);
        cache.Run.ExperienceRecordAdd(EExperienceRecordType.MonsterKill, "Slime");

        cache.ResetRun();

        Assert.AreEqual(0, cache.Run.Experience.Get(EExperienceRecordType.MonsterKill, "Slime"));
        Assert.AreEqual(3, cache.Slot.Experience.Get(EExperienceRecordType.MonsterKill, "Slime"));
        Assert.IsTrue(cache.Slot.HasSeen("EventA"));
    }

    [Test]
    public void RunCommands_RegisterCommandsAndSlotCompletionOptions()
    {
        var terminal = new JxTerminal();
        JxCommandAttributeRegistrar.RegisterCommands(terminal, new RunCommands(null));

        Assert.Contains("Save Status", terminal.GetCommandKeys().ToList());
        Assert.Contains("Save Slot Progress", terminal.GetCommandKeys().ToList());
        Assert.Contains("Experience Get", terminal.GetCommandKeys().ToList());
        Assert.Contains("Experience List", terminal.GetCommandKeys().ToList());
        CollectionAssert.AreEquivalent(new[] { "1", "2", "3" }, terminal.Complete("Select Save Slot ").Candidates);
    }

    [Test]
    public void AreaEventProgress_InitializesAtSavedAreaID()
    {
        var first = ScriptableObject.CreateInstance<AreaEventData>();
        var saved = ScriptableObject.CreateInstance<AreaEventData>();
        first.Id = "Area-1";
        first.Stage = 1;
        first.Area = 1;
        saved.Id = "Area-2";
        saved.Stage = 1;
        saved.Area = 2;

        try
        {
            var progress = new AreaEventProgress();
            var initialized = progress.TryInitialize(new[] { saved, first }, saved.Id);

            Assert.IsTrue(initialized);
            Assert.AreEqual(saved.Id, progress.Current.Id);
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(saved);
        }
    }

    [Test]
    public void TogSaveData_JsonRoundTripPreservesAreaVisits()
    {
        var source = new TogSaveData
        {
            activeRun = new ActiveRunSaveData
            {
                areaVisits = new List<AreaVisitSaveData>
                {
                    new() { areaID = "Area-1", eventType = (int)AreaEventType.Battle },
                    new() { areaID = "Area-2", eventType = (int)AreaEventType.Shop }
                }
            }
        };

        var restored = JsonUtility.FromJson<TogSaveData>(JsonUtility.ToJson(source));

        Assert.AreEqual(2, restored.activeRun.areaVisits.Count);
        Assert.AreEqual("Area-2", restored.activeRun.areaVisits[1].areaID);
        Assert.AreEqual(AreaEventType.Shop, (AreaEventType)restored.activeRun.areaVisits[1].eventType);
    }

    [Test]
    public void AreaEventProgress_StageNodesKeepPositionUntilNextAreaIsEntered()
    {
        var areas = CreateStageAreas();

        try
        {
            var progress = new AreaEventProgress();
            Assert.IsTrue(progress.TryInitialize(areas));
            Assert.IsTrue(progress.TryBeginEvent(AreaEventType.Battle));
            Assert.IsTrue(progress.TryCompleteEvent(AreaEventType.Battle, true));

            var selecting = progress.GetStageNodes();
            Assert.AreEqual(EStageNodeState.Current, selecting[0].State);
            Assert.AreEqual(EStageNodeState.Locked, selecting[1].State);
            Assert.IsFalse(selecting[0].IsPathSolid);

            Assert.IsTrue(progress.TryBeginEvent(AreaEventType.Shop));

            var entered = progress.GetStageNodes();
            Assert.AreEqual(EStageNodeState.Visited, entered[0].State);
            Assert.AreEqual(AreaEventType.Battle, entered[0].EventType);
            Assert.IsTrue(entered[0].IsPathSolid);
            Assert.AreEqual(EStageNodeState.Current, entered[1].State);
            Assert.AreEqual(AreaEventType.Shop, entered[1].EventType);
            Assert.IsFalse(entered[1].IsPathSolid);
            Assert.AreEqual(EStageNodeState.Locked, entered[2].State);
            Assert.IsNull(entered[2].EventType);
            Assert.IsTrue(entered[2].IsBoss);
        }
        finally
        {
            DestroyAreas(areas);
        }
    }

    [Test]
    public void AreaEventProgress_StageNodesRestoreSavedVisits()
    {
        var areas = CreateStageAreas();

        try
        {
            var saved = new Dictionary<string, AreaEventType>
            {
                { areas[0].Id, AreaEventType.Battle },
                { areas[1].Id, AreaEventType.Random }
            };
            var progress = new AreaEventProgress();
            Assert.IsTrue(progress.TryInitialize(areas, areas[2].Id, saved));

            var nodes = progress.GetStageNodes();
            Assert.AreEqual(EStageNodeState.Visited, nodes[0].State);
            Assert.AreEqual(AreaEventType.Battle, nodes[0].EventType);
            Assert.AreEqual(EStageNodeState.Current, nodes[1].State);
            Assert.AreEqual(AreaEventType.Random, nodes[1].EventType);
            Assert.AreEqual(EStageNodeState.Locked, nodes[2].State);
        }
        finally
        {
            DestroyAreas(areas);
        }
    }

    [Test]
    public void AreaEventProgress_StageNodesIncludeOnlyEnteredStage()
    {
        var areas = CreateStageAreas();
        var nextStage = ScriptableObject.CreateInstance<AreaEventData>();
        nextStage.Id = "Area-4";
        nextStage.Stage = 2;
        nextStage.Area = 1;
        areas.Add(nextStage);

        try
        {
            var progress = new AreaEventProgress();
            Assert.IsTrue(progress.TryInitialize(areas));

            Assert.AreEqual(3, progress.GetStageNodes().Count);
        }
        finally
        {
            DestroyAreas(areas);
        }
    }

    private static List<AreaEventData> CreateStageAreas()
    {
        var areas = new List<AreaEventData>();
        for (int i = 1; i <= 3; i++)
        {
            var area = ScriptableObject.CreateInstance<AreaEventData>();
            area.Id = $"Area-{i}";
            area.Stage = 1;
            area.Area = i;
            area.BossEvent = i == 3 ? 100 : 0;
            areas.Add(area);
        }
        return areas;
    }

    private static void DestroyAreas(IEnumerable<AreaEventData> areas)
    {
        foreach (var area in areas)
        {
            Object.DestroyImmediate(area);
        }
    }

    [Test]
    public void AreaEventProgress_InitialBattleCanCompleteAfterCheckpointInitialization()
    {
        var first = ScriptableObject.CreateInstance<AreaEventData>();
        var second = ScriptableObject.CreateInstance<AreaEventData>();
        first.Id = "Area-1";
        first.Stage = 1;
        first.Area = 1;
        second.Id = "Area-2";
        second.Stage = 1;
        second.Area = 2;

        try
        {
            var progress = new AreaEventProgress();
            Assert.IsTrue(progress.TryInitialize(new[] { first, second }));
            Assert.IsTrue(progress.TryBeginEvent(AreaEventType.Battle));
            Assert.IsTrue(progress.TryCompleteEvent(AreaEventType.Battle, true));
            Assert.AreEqual(second.Id, progress.Current.Id);
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }
    }
}
