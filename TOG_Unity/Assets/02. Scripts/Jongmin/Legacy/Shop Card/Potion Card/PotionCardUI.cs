using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionCardUI : MonoBehaviour, IPotionCardUI
{
    [Header("UI References")]
    [SerializeField] private GameObject alreadyPurchasedImage;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text costLabel;

    private PotionCardPresenter _potionCardPresenter;

    private void OnDisable()
        => _potionCardPresenter?.Dispose();

    public void Inject(PotionCardPresenter potionCardPresenter)
    {
        _potionCardPresenter = potionCardPresenter;

        purchaseButton.onClick.AddListener(_potionCardPresenter.OnClickedPurchase);
    }

    public void InitUI(int cost, bool canPurchase)
    {
        alreadyPurchasedImage.SetActive(false);

        costLabel.text = canPurchase ? $"{cost}G"
                                      : $"<color=red>{cost}G</color>";

        purchaseButton.interactable = canPurchase;
        SetPurchaseButtonAlpha(1f);
    }

    public void UpdateUI(int cost, bool canPurchase)
    {
        if(_potionCardPresenter.Purchased)
        {
            costLabel.text = string.Empty;

            purchaseButton.interactable = false;
            SetPurchaseButtonAlpha(0f);

            alreadyPurchasedImage.SetActive(_potionCardPresenter.Purchased);
        }
        else
        {
            InitUI(cost, canPurchase);
        }
    }

    private void SetPurchaseButtonAlpha(float alpha)
    {
        var color = purchaseButton.image.color;
        color.a = alpha;
        purchaseButton.image.color = color;
    }
}
