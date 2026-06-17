public class CraftmanPresenter
{
    private readonly ICraftmanUI _craftmanUI;
    private readonly CraftmanDeckInvenPresenter _craftmanDeckInvenPresenter;

    public CraftmanPresenter(ICraftmanUI craftmanUI,
                             CraftmanDeckInvenPresenter craftmanDeckInvenPresenter)
    {
        _craftmanUI = craftmanUI;
        _craftmanDeckInvenPresenter = craftmanDeckInvenPresenter;
    }

    public void OpenUI()
    {
        _craftmanUI.OpenUI();
        _craftmanDeckInvenPresenter.OpenUI();
    }

    public void CloseUI()
    {
        _craftmanUI.CloseUI();
        _craftmanDeckInvenPresenter.CloseUI();
    }
}
