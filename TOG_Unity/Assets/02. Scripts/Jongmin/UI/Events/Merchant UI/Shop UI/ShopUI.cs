using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopUI : MonoBehaviour, IShopUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Button saleButton;
    [SerializeField] private TMP_Text saleLabel;
    
    [Header("Animation References")]
    [SerializeField] private float animationDuration = 0.5f;

    public void Construct(ShopPresenter shopPresenter)
        => saleButton.onClick.AddListener(shopPresenter.OnClickedSale);

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    public void ToggleSaleButton(bool isActive)
    {
        saleButton.interactable = isActive;

        saleLabel.text = isActive ? "카드 판매"
                                  : "<color=red>카드 판매</color>";
    }

    private void ToggleUI(bool isActive)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(isActive ? 1f : 0f, animationDuration);
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;
    }
}
