using NUnit.Framework;

public class BloodSuckingEffectTests
{
    [Test]
    public void CalculateHeal_UsesAttackPowerPercentOnly()
    {
        Assert.AreEqual(1, BloodSuckingEffectSpawner.CalculateHeal(50, 2));
        Assert.AreEqual(3, BloodSuckingEffectSpawner.CalculateHeal(100, 3));
        Assert.AreEqual(20, BloodSuckingEffectSpawner.CalculateHeal(200, 10));
    }

    [Test]
    public void CalculateHeal_SmallAttackStillHealsAtLeastOne()
    {
        Assert.AreEqual(1, BloodSuckingEffectSpawner.CalculateHeal(10, 1));
        Assert.AreEqual(1, BloodSuckingEffectSpawner.CalculateHeal(20, 1));
        Assert.AreEqual(1, BloodSuckingEffectSpawner.CalculateHeal(30, 2));
    }

    [Test]
    public void CalculateHeal_DoesNotIncludeSeparateSynergyDamage()
    {
        int attackPower = 40;
        int overwhelmingDamage = 20;

        Assert.AreEqual(1, BloodSuckingEffectSpawner.CalculateHeal(attackPower, 2));
        Assert.AreNotEqual(
            BloodSuckingEffectSpawner.CalculateHeal(attackPower + overwhelmingDamage, 2),
            BloodSuckingEffectSpawner.CalculateHeal(attackPower, 2));
    }

    [Test]
    public void CalculateHeal_ReturnsZeroWhenAttackOrPercentIsMissing()
    {
        Assert.AreEqual(0, BloodSuckingEffectSpawner.CalculateHeal(0, 2));
        Assert.AreEqual(0, BloodSuckingEffectSpawner.CalculateHeal(50, 0));
        Assert.AreEqual(0, BloodSuckingEffectSpawner.CalculateHeal(-10, 2));
    }

    [Test]
    public void CanApplyHeal_SkipsWhenHealthIsFull_ButAmountIsStillPositive()
    {
        Assert.IsFalse(BloodSuckingEffectSpawner.CanApplyHeal(100, 100, 1));
        Assert.IsTrue(BloodSuckingEffectSpawner.CanApplyHeal(99, 100, 1));
        Assert.IsFalse(BloodSuckingEffectSpawner.CanApplyHeal(50, 100, 0));
    }
}
