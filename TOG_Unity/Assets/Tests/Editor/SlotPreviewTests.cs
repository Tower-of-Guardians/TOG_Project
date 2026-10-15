using System;
using System.Collections.Generic;
using System.Globalization;
using Jongmin;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class SlotPreviewTests
{
    [Test]
    public void SlotValidation_RejectsMissingDataAndInvalidMetadata()
    {
        Assert.IsFalse(SlotSaveDataReader.IsValid(null));
        Assert.IsFalse(SlotSaveDataReader.IsValid(new TogSaveData { slotInfo = null }));
        Assert.IsFalse(SlotSaveDataReader.IsValid(new TogSaveData { slotProgress = null }));
        Assert.IsFalse(SlotSaveDataReader.IsValid(new TogSaveData { version = -1 }));
        Assert.IsFalse(SlotSaveDataReader.IsValid(new TogSaveData { runCount = -1 }));
        var data = new TogSaveData();
        data.slotInfo.savedAt = long.MaxValue;
        Assert.IsFalse(SlotSaveDataReader.IsValid(data));
        data.slotInfo.savedAt = 0;
        data.slotInfo.playTime = -1;
        Assert.IsFalse(SlotSaveDataReader.IsValid(data));
    }

    [Test]
    public void SlotValidation_AcceptsLobbyAndRunButRejectsBrokenRun()
    {
        var data = new TogSaveData();
        Assert.IsTrue(SlotSaveDataReader.IsValid(data));
        data.activeRun = new ActiveRunSaveData { runID = "run-1" };
        Assert.IsTrue(SlotSaveDataReader.IsValid(data));
        data.activeRun.checkpointType = (ERunCheckpointType)999;
        Assert.IsFalse(SlotSaveDataReader.IsValid(data));
        data.activeRun.checkpointType = ERunCheckpointType.AreaEntry;
        data.activeRun.player = null;
        Assert.IsFalse(SlotSaveDataReader.IsValid(data));
    }

    [Test]
    public void Preview_ReadsSlotTotalsAndFormatsDateAndLongPlayTime()
    {
        var date = new DateTimeOffset(2026, 10, 12, 10, 30, 0, TimeSpan.Zero);
        var data = new TogSaveData { runCount = 12 };
        data.slotInfo.savedAt = date.ToUnixTimeSeconds();
        data.slotInfo.playTime = 27 * 3600 + 20 * 60 + 59;
        data.slotProgress.relicIDs = new List<string> { "A", "A", "B", "", null };
        var preview = new SlotPreview(1, data, "로비");

        Assert.IsFalse(preview.IsEmpty);
        Assert.AreEqual(2, preview.RelicCount);
        Assert.AreEqual(12, preview.RunCount);
        Assert.AreEqual("27 시간 20 분", preview.PlayTimeText);
        Assert.AreEqual(date.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), preview.SavedAtText);
        var restored = JsonUtility.FromJson<TogSaveData>(JsonUtility.ToJson(data));
        Assert.AreEqual(12, restored.runCount);
        Assert.AreEqual(data.slotInfo.playTime, restored.slotInfo.playTime);
    }

    [Test]
    public void Preview_EmptyAndLobbyAndAreaAreDistinct()
    {
        Assert.IsTrue(new SlotPreview(0, null).IsEmpty);
        var data = new TogSaveData();
        Assert.IsFalse(new SlotPreview(0, data).IsEmpty);
        Assert.AreEqual("로비", SlotPreviewSource.GetLocation(data));
        data.activeRun = new ActiveRunSaveData { areaID = "test-area-unknown" };
        Assert.AreEqual("지역 test-area-unknown", SlotPreviewSource.GetLocation(data));
    }

    [Test]
    public void Clock_CountsOnlySelectedSessionWithoutDoubleCountingOrLostFractions()
    {
        var clock = new SlotPlayClock();
        Assert.AreEqual(0, clock.CollectSeconds(10));
        clock.Start(10);
        Assert.AreEqual(0, clock.CollectSeconds(10.5));
        Assert.AreEqual(1, clock.CollectSeconds(11.25));
        Assert.AreEqual(0, clock.CollectSeconds(11.25));
        Assert.AreEqual(2, clock.CollectSeconds(13));
        clock.Stop();
        Assert.AreEqual(0, clock.CollectSeconds(100));
        clock.Start(200);
        Assert.AreEqual(2, clock.CollectSeconds(202));
    }

    [Test]
    public void Prefab_AllCardsSwitchBetweenDetailsAndExistingEmptyState()
    {
        var root = PrefabUtility.LoadPrefabContents("Assets/03. Prefabs/Main/PF_Title.prefab");
        try
        {
            var cards = root.GetComponentsInChildren<SlotCardView>(true);
            Assert.AreEqual(3, cards.Length);
            foreach (var card in cards)
            {
                var serialized = new SerializedObject(card);
                var details = (GameObject)serialized.FindProperty("_detailsRoot").objectReferenceValue;
                var empty = (GameObject)serialized.FindProperty("_emptyRoot").objectReferenceValue;
                foreach (var fieldName in new[] { "Location", "RelicCount", "RunCount", "SavedAt", "PlayTime" })
                {
                    Assert.IsNotNull(details.transform.Find(fieldName + "Caption"));
                    Assert.IsNotNull(details.transform.Find(fieldName + "Value"));
                }
                card.Bind(new SlotPreview(0, new TogSaveData { runCount = 12 }, "로비"), _ => { });
                Assert.IsTrue(details.activeSelf);
                Assert.IsFalse(empty.activeSelf);
                var label = (TMPro.TMP_Text)serialized.FindProperty("_runCountLabel").objectReferenceValue;
                Assert.AreEqual("12", label.text);
                card.Bind(new SlotPreview(0, null), _ => { });
                Assert.IsFalse(details.activeSelf);
                Assert.IsTrue(empty.activeSelf);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
