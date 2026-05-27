using System;
using VContainer.Unity;

public class StatusPresenter : IDisposable, IInitializable
{
    private readonly IStatusUI _statusUI;

    public StatusPresenter(IStatusUI statusUI)
        => _statusUI = statusUI;
    
    public void Initialize()
    {
        DataCenter.Instance.playerLevelEvent += UpdateLevel;
        DataCenter.Instance.playerMoneyEvent += UpdateGold;
        DataCenter.Instance.playerStateEvent += Initialize;
        DataCenter.Instance.SetPlayerState();
    }

    private void Initialize(PlayerState playerState)
    {
        UpdateLevel(playerState.level, playerState.experience);
        UpdateGold(playerState.money);
    }

    public void UpdateLevel(int level, int exp)
    {
        float expRatio = (float)exp / DataCenter.Instance.playerstate.maxexperience;
        _statusUI.UpdateLevel(level, expRatio);
    }

    public void UpdateGold(int gold)
        => _statusUI.UpdateGold(gold);

    public void Dispose()
    {
        DataCenter.Instance.playerLevelEvent -= UpdateLevel;
        DataCenter.Instance.playerMoneyEvent -= UpdateGold;
        DataCenter.Instance.playerStateEvent -= Initialize;
    }
}
