using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardUI : CardUI, IShopCardUI
{
    [Space(30f), Header("UI References")]
    [SerializeField] private TMP_Text costLabel;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private GameObject alreadyPurchasedImage;

    private ShopCardPresenter _shopCardPresenter;

    private void OnDisable()
        => _shopCardPresenter?.Dispose();

    public void Construct(ShopCardPresenter presenter)
    {
        _shopCardPresenter = presenter;

        purchaseButton.onClick.AddListener(_shopCardPresenter.OnClickedPurchase);
    }

    public void InitUI(BattleCardData cardData, bool canPurchase)
    {
        UpdateUI(cardData.data);   
        SetPurchaseButtonAlpha(1f);   
        alreadyPurchasedImage.SetActive(false);  

        costLabel.text = canPurchase ? $"{cardData.data.price}G"
                                      : $"<color=red>{cardData.data.price}G</color>";

        purchaseButton.interactable = canPurchase;
    }

    public void UpdateUI(BattleCardData cardData, bool canPurchase)
    {
        if(_shopCardPresenter.Purchased)
        {
            costLabel.text = string.Empty;
            
            purchaseButton.interactable = false;
            SetPurchaseButtonAlpha(0f);

            alreadyPurchasedImage.SetActive(_shopCardPresenter.Purchased);
        }
        else
        {
            InitUI(cardData, canPurchase);
        }
    }

    private void SetPurchaseButtonAlpha(float alpha)
    {
        var color = purchaseButton.image.color;
        color.a = alpha;
        purchaseButton.image.color = color;
    }
}
