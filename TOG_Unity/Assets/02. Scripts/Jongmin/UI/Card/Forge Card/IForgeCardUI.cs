public interface IForgeCardUI : ICardUI
{
    void UpgradeATK(float atk);
    void UpgradeBoth(float atk, float def);
    void UpgradeDEF(float def);
}