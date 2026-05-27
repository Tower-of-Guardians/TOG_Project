using VContainer.Unity;

public class ResultPresenter : IInitializable
{
    private readonly IResultUI _resultUI;
    private readonly ResultRewardPresenter _resultRewardPresenter;
    private readonly ResultShopPresenter _resultShopPresenter;
    private readonly ResultDeckInvenPresenter _resultDeckInvenPresenter;

    public ResultPresenter(IResultUI resultUI,
                           ResultRewardPresenter resultRewardPresenter,
                           ResultShopPresenter resultShopPresenter,
                           ResultDeckInvenPresenter resultDeckInvenPresenter)
    {
        _resultUI = resultUI;
        _resultRewardPresenter = resultRewardPresenter;
        _resultShopPresenter = resultShopPresenter;
        _resultDeckInvenPresenter = resultDeckInvenPresenter;
    }

    public void Initialize()
        => _resultUI.Construct(this);

    /// <summary>
    /// UI 애니메이션 상태를 초기화합니다.
    /// </summary>
    public void InitUI()
        => _resultUI.Initialize();
    
    public void OpenUI(ResultData resultData)
        => _resultUI.OpenUI();

    public void CloseUI()
    {
        _resultUI.CloseUI();
        _resultRewardPresenter.CloseUI();
        _resultShopPresenter.CloseUI();
        _resultDeckInvenPresenter.CloseUI();
    }

    public void ShowCloseButton()
        => _resultUI.ShowCloseButton();
}
