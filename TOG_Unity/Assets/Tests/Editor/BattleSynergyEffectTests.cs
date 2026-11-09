using NUnit.Framework;

public class BattleSynergyEffectTests
{
    [Test]
    public void CalculateGoldPercentDamage_UsesRoundedPercentOfCurrentGold()
    {
        Assert.AreEqual(10, BattleCombatController.CalculateGoldPercentDamage(250, 4));
        Assert.AreEqual(20, BattleCombatController.CalculateGoldPercentDamage(250, 8));
        Assert.AreEqual(30, BattleCombatController.CalculateGoldPercentDamage(250, 12));
    }

    [Test]
    public void CalculateGoldPercentDamage_ZeroGoldOrPercent_ReturnsZero()
    {
        Assert.AreEqual(0, BattleCombatController.CalculateGoldPercentDamage(0, 4));
        Assert.AreEqual(0, BattleCombatController.CalculateGoldPercentDamage(250, 0));
        Assert.AreEqual(0, BattleCombatController.CalculateGoldPercentDamage(-10, 4));
    }

    [Test]
    public void ResolveNextTurnDrawCount_AddsBonusToMaxHandCount()
    {
        Assert.AreEqual(-1, BattleCombatController.ResolveNextTurnDrawCount(5, 0));
        Assert.AreEqual(6, BattleCombatController.ResolveNextTurnDrawCount(5, 1));
    }
}
