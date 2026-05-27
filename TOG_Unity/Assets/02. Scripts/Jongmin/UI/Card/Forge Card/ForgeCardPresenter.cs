public class ForgeCardPresenter : CardPresenter
{
    private readonly IForgeCardUI _forgeCardUI;
    
    public CardData CardData { get; private set; }

    public ForgeCardPresenter(IForgeCardUI forgeCardUI)
        => _forgeCardUI = forgeCardUI;

    public void Construct(CardData cardData)
    {
        CardData = cardData;
        _forgeCardUI.UpdateUI(cardData);
    }

    public void ATKUpgrade(float atk)
    {
        CardData.ATK += atk;
        _forgeCardUI.UpgradeATK(CardData.ATK);
    }

    public void BothUpgrade(float atk, float def)
    {
        CardData.ATK += atk;
        CardData.DEF += def;
        _forgeCardUI.UpgradeBoth(CardData.ATK, CardData.DEF);
    }

    public void DEFUpgrade(float def)
    {
        CardData.DEF += def;
        _forgeCardUI.UpgradeDEF(CardData.DEF);
    }
}
