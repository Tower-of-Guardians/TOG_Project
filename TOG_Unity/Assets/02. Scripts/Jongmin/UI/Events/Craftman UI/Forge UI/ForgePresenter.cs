using VContainer.Unity;
using System;

public class ForgePresenter : IInitializable, IDisposable
{
    private readonly IForgeUI _forgeUI;
    private readonly ForgeCardPresenter _forgeCardPresenter;
    private readonly IForgeDatabase _forgeDB;
    private readonly CraftmanPresenter _craftmanPresenter;
    private readonly CraftmanDeckInvenPresenter _craftmanDeckInvenPresenter;

    public event Action OnReinforceCompleted;

    public ForgePresenter(IForgeUI forgeUI,
                          IForgeDatabase forgeDB,
                          ForgeCardPresenter forgeCardPresenter,
                          CraftmanPresenter craftmanPresenter,
                          CraftmanDeckInvenPresenter craftmanDeckInvenPresenter)
    {
        _forgeUI = forgeUI;
        _forgeDB = forgeDB;
        _forgeCardPresenter = forgeCardPresenter;
        _craftmanPresenter = craftmanPresenter;
        _craftmanDeckInvenPresenter = craftmanDeckInvenPresenter;
    }

    public void Initialize()
    {
        _craftmanDeckInvenPresenter.OnCardSelected += OpenUI;
        _forgeUI.Construct(this);
    }

    public void Dispose()
    {
        _craftmanDeckInvenPresenter.OnCardSelected -= OpenUI;
    }
    
    public void OpenUI(CardData cardData)
    {
        _forgeCardPresenter.Construct(cardData);
        _forgeUI.OpenUI();
        _forgeUI.ToggleButtonGroup(true);
    }

    public void CloseUI()
        => _forgeUI.CloseUI();

    public void OnClickedAtkUpgrade()
    {
        // TODO: 현재 스테이지만큼으로 조정
        ForgeData forgeData = _forgeDB.GetForgeData(1);
        _forgeCardPresenter.ATKUpgrade(forgeData.ATK);
        
        _forgeUI.ToggleCloseButton(true);
        _forgeUI.ToggleButtonGroup(false);
        
        OnReinforceCompleted?.Invoke();
    }

    public void OnClickedBothUpgrade()
    {
        // TODO: 현재 스테이지만큼으로 조정
        ForgeData forgeData = _forgeDB.GetForgeData(1);
        _forgeCardPresenter.BothUpgrade(forgeData.ATK * 0.5f, forgeData.DEF * 0.5f);
        
        _forgeUI.ToggleCloseButton(true);
        _forgeUI.ToggleButtonGroup(false);
        
        OnReinforceCompleted?.Invoke();
    }

    public void OnClickedDefUpgrade()
    {
        // TODO: 현재 스테이지만큼으로 조정
        ForgeData forgeData = _forgeDB.GetForgeData(1);
        _forgeCardPresenter.DEFUpgrade(forgeData.DEF);
        
        _forgeUI.ToggleCloseButton(true);
        _forgeUI.ToggleButtonGroup(false);
        
        OnReinforceCompleted?.Invoke();
    }

    public void OnClickedCancel()
    {
        _craftmanDeckInvenPresenter.OpenUI();
        _forgeUI.CloseUI();
    }

    public void OnClickedClose()
    {
        _forgeUI.CloseUI();
        _craftmanPresenter.CloseUI();
        _craftmanDeckInvenPresenter.CloseUI();
    }
}
