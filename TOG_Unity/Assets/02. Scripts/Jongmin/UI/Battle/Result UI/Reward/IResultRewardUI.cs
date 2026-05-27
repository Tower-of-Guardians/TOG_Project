public interface IResultRewardUI : IOpenableUI
{
    void Initialize();
    void UpdateUI(int gold, int exp, bool isLevelUp);
}