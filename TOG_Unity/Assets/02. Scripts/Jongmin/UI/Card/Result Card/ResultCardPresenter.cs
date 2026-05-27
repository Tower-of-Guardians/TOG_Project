public class ResultCardPresenter : CardPresenter
{
    private readonly IResultCardUI _resultCardUI;
    
    private bool _isPurchased;

    public ResultCardPresenter(IResultCardUI resultCardUI, 
                               BattleCardData battleCardData)
    {
        _resultCardUI = resultCardUI;
        BattleCardData = battleCardData;
        
        _resultCardUI.Construct(this);
        _resultCardUI.UpdateUI(battleCardData.data);
        UpdateState(DataCenter.Instance.playerstate.money);
    }

    /// <summary>
    /// 이미 판매가 된 카드가 아니라면 money에 맞춰 구매 가능 여부에 따라 상태를 갱신합니다.
    /// </summary>
    public void UpdateState(int money)
    {
        if (_isPurchased)
        {
            return;
        }
        
        bool canPurchase = money >= BattleCardData.data.price;
        _resultCardUI.UpdatePurchase(false);
        _resultCardUI.UpdateUI(BattleCardData, canPurchase);
    }

    public void OnClickedPurchase()
    {
        if (_isPurchased)
        {
            return;
        }
        
        if (DataCenter.Instance.playerstate.money >= BattleCardData.data.price)
        {
            _isPurchased = true;
            _resultCardUI.UpdatePurchase(true);
            DataCenter.Instance.SetMoney(-BattleCardData.data.price);
        }
    }
}
