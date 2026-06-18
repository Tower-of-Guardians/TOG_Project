using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultCardUI : CardUI, IResultCardUI
{
    [Header("UI References")]
    [SerializeField] private Button purchaseButton;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject soldOutPanel;
    
    [Header("Text References")]
    [SerializeField] private TMP_Text costLabel;
    
    private ResultCardPresenter _resultCardPresenter;

    public void Construct(ResultCardPresenter resultCardPresenter)
    {
        _resultCardPresenter = resultCardPresenter;
        purchaseButton.onClick.AddListener(_resultCardPresenter.OnClickedPurchase);
    }

    private void OnDisable()
    {
        if (_resultCardPresenter == null)
        {
            return;
        }
        
        purchaseButton.onClick.RemoveListener(_resultCardPresenter.OnClickedPurchase);
    }

    public void UpdateUI(BattleCardData battleCardData, bool canPurchase)
    {
        costLabel.text = canPurchase ? $"${battleCardData.data.price}"
                                     : $"<color=red>${battleCardData.data.price}</color>";
        purchaseButton.interactable = canPurchase;
    }

    public void UpdatePurchase(bool isSoldOut)
    {
        canvasGroup.alpha = isSoldOut ? 0f : 1f;
        canvasGroup.blocksRaycasts = !isSoldOut;
        canvasGroup.interactable = !isSoldOut;
        
        soldOutPanel.SetActive(isSoldOut);
    }
}
