using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SynergyActivationSelectorTests
{
    [Test]
    public void Select_EmptyList_ReturnsEmptyAndZeroLoops()
    {
        List<SynergyTotalData> selected = SynergyActivationSelector.Select(new List<SynergyTotalData>());

        Assert.IsEmpty(selected);
        Assert.AreEqual(0, SynergyActivationSelector.GetLoopPlayCount(selected.Count));
    }

    [Test]
    public void Select_OneActivatedSynergy_ReturnsOneAndOneLoop()
    {
        List<SynergyTotalData> selected = SynergyActivationSelector.Select(new[]
        {
            CreateEntry("honesty", tier: 1, count: 2, effectValue: 2)
        });

        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("honesty", selected[0].synergyData.ID);
        Assert.AreEqual(1, SynergyActivationSelector.GetLoopPlayCount(selected.Count));
    }

    [Test]
    public void Select_TwoActivatedSynergies_PlaysLoopTwice()
    {
        List<SynergyTotalData> selected = SynergyActivationSelector.Select(new[]
        {
            CreateEntry("honesty", tier: 1, count: 2, effectValue: 2),
            CreateEntry("shield", tier: 2, count: 2, effectValue: 1)
        });

        Assert.AreEqual(2, selected.Count);
        Assert.AreEqual(2, SynergyActivationSelector.GetLoopPlayCount(selected.Count));
    }

    [Test]
    public void Select_MoreThanThreeActivated_KeepsHighestPriorityThree()
    {
        List<SynergyTotalData> selected = SynergyActivationSelector.Select(new[]
        {
            CreateEntry("overflow", tier: 10, count: 1, effectValue: 1),
            CreateEntry("honesty", tier: 1, count: 4, effectValue: 10),
            CreateEntry("shield", tier: 2, count: 3, effectValue: 2),
            CreateEntry("plunder", tier: 3, count: 3, effectValue: 5)
        });

        Assert.AreEqual(3, selected.Count);
        Assert.AreEqual("honesty", selected[0].synergyData.ID);
        Assert.AreEqual("shield", selected[1].synergyData.ID);
        Assert.AreEqual("plunder", selected[2].synergyData.ID);
        Assert.AreEqual(3, SynergyActivationSelector.GetLoopPlayCount(selected.Count));
    }

    [Test]
    public void Select_IgnoresInactiveSynergies_EvenWhenTheyHaveHigherCount()
    {
        List<SynergyTotalData> selected = SynergyActivationSelector.Select(new[]
        {
            CreateEntry("inactiveHighCount", tier: 1, count: 5, effectValue: 0),
            CreateEntry("activeLowCount", tier: 8, count: 2, effectValue: 4)
        });

        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("activeLowCount", selected[0].synergyData.ID);
    }

    [Test]
    public void Select_IgnoresInactiveSynergies()
    {
        List<SynergyTotalData> selected = SynergyActivationSelector.Select(new[]
        {
            CreateEntry("inactive", tier: 1, count: 1, effectValue: 0),
            CreateEntry("active", tier: 5, count: 2, effectValue: 4)
        });

        Assert.AreEqual(1, selected.Count);
        Assert.AreEqual("active", selected[0].synergyData.ID);
    }

    [Test]
    public void Select_OrdersByCountThenLowerTier()
    {
        List<SynergyTotalData> selected = SynergyActivationSelector.Select(new[]
        {
            CreateEntry("lowCountHighTier", tier: 8, count: 2, effectValue: 1),
            CreateEntry("highCount", tier: 9, count: 4, effectValue: 1),
            CreateEntry("sameCountLowerTier", tier: 2, count: 2, effectValue: 1)
        });

        Assert.AreEqual("highCount", selected[0].synergyData.ID);
        Assert.AreEqual("sameCountLowerTier", selected[1].synergyData.ID);
        Assert.AreEqual("lowCountHighTier", selected[2].synergyData.ID);
    }

    [Test]
    public void GetLoopPlayCount_ClampsToMaxActivations()
    {
        Assert.AreEqual(0, SynergyActivationSelector.GetLoopPlayCount(0));
        Assert.AreEqual(1, SynergyActivationSelector.GetLoopPlayCount(1));
        Assert.AreEqual(2, SynergyActivationSelector.GetLoopPlayCount(2));
        Assert.AreEqual(3, SynergyActivationSelector.GetLoopPlayCount(3));
        Assert.AreEqual(3, SynergyActivationSelector.GetLoopPlayCount(8));
    }

    [Test]
    public void GetMinimumActivationCount_ReturnsFirstCountWithEffect()
    {
        SynergyData data = ScriptableObject.CreateInstance<SynergyData>();
        data.Effect1Synergys = new List<int> { 0, 0, 1, 0, 0 };

        Assert.AreEqual(3, SynergyActivationSelector.GetMinimumActivationCount(data));
    }

    private static SynergyTotalData CreateEntry(string id, int tier, int count, int effectValue)
    {
        SynergyData data = ScriptableObject.CreateInstance<SynergyData>();
        data.ID = id;
        data.Tier = tier;
        data.Effect1Synergys = new List<int> { effectValue };

        return new SynergyTotalData
        {
            synergyData = data,
            count = count
        };
    }
}
