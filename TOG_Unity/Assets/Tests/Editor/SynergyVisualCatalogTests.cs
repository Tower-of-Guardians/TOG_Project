using NUnit.Framework;
using UnityEngine;

public class SynergyVisualCatalogTests
{
    [Test]
    public void TryGetEffectStateName_MappedSynergies_HaveEffectClips()
    {
        Assert.IsTrue(SynergyVisualCatalog.TryGetEffectStateName(SynergyVisualCatalog.BasicId, out string basicState));
        Assert.AreEqual(SynergyVisualCatalog.BasicEffectStateName, basicState);

        Assert.IsTrue(SynergyVisualCatalog.TryGetEffectStateName(SynergyVisualCatalog.BloodSuckingId, out string bloodState));
        Assert.AreEqual(SynergyVisualCatalog.BloodSuckingEffectStateName, bloodState);

        Assert.IsTrue(SynergyVisualCatalog.TryGetEffectStateName(SynergyVisualCatalog.ShieldAttackId, out string shieldState));
        Assert.AreEqual(SynergyVisualCatalog.ShieldAttackEffectStateName, shieldState);

        Assert.IsTrue(SynergyVisualCatalog.TryGetEffectStateName(SynergyVisualCatalog.PlunderId, out string plunderState));
        Assert.AreEqual(SynergyVisualCatalog.PlunderEffectStateName, plunderState);

        Assert.IsTrue(SynergyVisualCatalog.TryGetEffectStateName(SynergyVisualCatalog.MysteryId, out string mysteryState));
        Assert.AreEqual(SynergyVisualCatalog.MysteryEffectStateName, mysteryState);
    }

    [Test]
    public void TryGetEffectStateName_OtherSynergies_HaveNoEffectClip()
    {
        Assert.IsFalse(SynergyVisualCatalog.TryGetEffectStateName(SynergyVisualCatalog.HonestyId, out _));
        Assert.IsFalse(SynergyVisualCatalog.TryGetEffectStateName(SynergyVisualCatalog.OverwhelmingId, out _));
    }

    [Test]
    public void GetAuraColor_UsesSpecifiedHexForBasicAndBloodSucking()
    {
        Assert.AreEqual(SynergyVisualCatalog.Hex("FFFBE6"), SynergyVisualCatalog.GetAuraColor(SynergyVisualCatalog.BasicId));
        Assert.AreEqual(SynergyVisualCatalog.Hex("BF0000"), SynergyVisualCatalog.GetAuraColor(SynergyVisualCatalog.BloodSuckingId));
    }

    [Test]
    public void GetAuraColor_UsesDistinctColorsPerSynergy()
    {
        Color basic = SynergyVisualCatalog.GetAuraColor(SynergyVisualCatalog.BasicId);
        Color blood = SynergyVisualCatalog.GetAuraColor(SynergyVisualCatalog.BloodSuckingId);
        Color honesty = SynergyVisualCatalog.GetAuraColor(SynergyVisualCatalog.HonestyId);

        Assert.AreNotEqual(basic, blood);
        Assert.AreNotEqual(basic, honesty);
        Assert.AreNotEqual(blood, honesty);
    }
}
