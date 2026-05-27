using System;
using UnityEngine;

public class ShopCardPresenter : CardPresenter, IDisposable
{
    private readonly IShopCardUI _shopCardUI;
    private readonly PlayerState _playerState;
    private readonly ShopDispenser _shopDispenser;

    public bool Purchased { get; private set; }

    public ShopCardPresenter(IShopCardUI shopCardUI,
                             ShopDispenser shopDispenser)
    {
        _shopCardUI = shopCardUI;
        _playerState = DataCenter.Instance.playerstate;
        _shopDispenser = shopDispenser;

        _shopCardUI.Construct(this);
    }

    public void Inject(BattleCardData battleCardData)
    {
        BattleCardData = battleCardData;
        Purchased = false;

        _shopDispenser.OnPurchasedAnyItem += UpdateUI;
        UpdateUI();
    }

    public void OnClickedPurchase()
    {
        int cardCost = BattleCardData.data.price;
        bool canPurchase = _playerState.money >= cardCost;

        if(canPurchase)
        {
            _playerState.money -= cardCost;
            DataCenter.Instance.userDeck.Add(BattleCardData.data);

            Purchased = true;
            _shopDispenser.Alert();
        }
    }
    
    private void UpdateUI()
    {
        int cardCost = BattleCardData.data.price;
        bool canPurchase = _playerState.money >= cardCost;
        
        _shopCardUI.UpdateUI(BattleCardData, canPurchase);
    }

    public void Dispose()
    {
        if (_shopDispenser != null)
        {
            _shopDispenser.OnPurchasedAnyItem -= UpdateUI;
        }
    }


}
