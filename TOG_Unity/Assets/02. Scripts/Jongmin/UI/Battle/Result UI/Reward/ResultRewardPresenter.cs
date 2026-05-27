using VContainer.Unity;

public class ResultRewardPresenter : IInitializable
{
    private readonly IResultRewardUI _resultRewardUI;

    public ResultRewardPresenter(IResultRewardUI resultRewardUI)
        => _resultRewardUI = resultRewardUI;
    
    public void Initialize() { }
    
    /// <summary>
    /// UI 애니메이션 상태를 초기화합니다.
    /// </summary>
    public void InitUI()
        => _resultRewardUI.Initialize();

    public void OpenUI(int gold, int exp)
    {
        _resultRewardUI.UpdateUI(gold, exp, true);
        _resultRewardUI.OpenUI();
    }

    public void CloseUI()
        => _resultRewardUI.CloseUI();
}
