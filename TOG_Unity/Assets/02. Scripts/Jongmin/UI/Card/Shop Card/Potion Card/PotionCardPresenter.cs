using System;

public class PotionCardPresenter : IDisposable
{
    private readonly IPotionCardUI _mCardUI;
    private readonly PlayerState _playerState;
    private readonly ShopDispenser _shopDispenser;

    private const int Cost = 40;
    private const float RecoveryRate = 0.2f;

    public bool Purchased { get; private set; }

    public PotionCardPresenter(IPotionCardUI cardUI,
                               ShopDispenser dispenser)
    {
        _mCardUI = cardUI;
        _playerState = DataCenter.Instance.playerstate;
        _shopDispenser = dispenser;

        _mCardUI.Inject(this);
    }

    public void Initialize()
    {
        Purchased = false;

        _shopDispenser.OnPurchasedAnyItem += UpdateUI;
        UpdateUI();
    }

    public void OnClickedPurchase()
    {
        if(_playerState.money < Cost)
            return;

        Purchased = true;

        _playerState.money -= Cost;
        _playerState.hp = (int)(_playerState.hp * RecoveryRate);

        _shopDispenser.Alert();
    }

    public void Dispose()
    {
        if (_shopDispenser != null)
        {
            _shopDispenser.OnPurchasedAnyItem -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        bool canPurchase = _playerState.money >= Cost;
        _mCardUI.UpdateUI(Cost, canPurchase);
    }
}
