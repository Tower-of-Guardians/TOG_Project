using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResultShopUI : MonoBehaviour, IResultShopUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button refreshButton;
    
    [Header("Text References")]
    [SerializeField] private TMP_Text rateLabel;
    [SerializeField] private TMP_Text refreshLabel;
    
    [Header("Animation References")]
    [SerializeField] private float animationDuration = 0.5f;
    
    private ResultShopPresenter _resultShopPresenter;
    
    public void Construct(ResultShopPresenter resultShopPresenter)
    {
        _resultShopPresenter = resultShopPresenter;
        refreshButton.onClick.AddListener(_resultShopPresenter.Refresh);
    }

    public void Initialize()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    public void UpdateRate(string rate_string)
        => rateLabel.text = rate_string;

    public void UpdateRefresh(string refreshString, bool canRefresh)
    {
        refreshLabel.text = canRefresh ? $"{refreshString}"
                                       : $"<color=red>{refreshString}</color>";

        refreshButton.interactable = canRefresh;
    }

    private void ToggleUI(bool isActive)
    {
        canvasGroup.DOKill();
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;

        if (isActive)
        {
            canvasGroup.DOFade(1f, animationDuration)
                       .OnComplete(() => { _resultShopPresenter.CreateAllCards(); });
        }
        else
        {
            canvasGroup.DOFade(0f, animationDuration)
                       .OnComplete(() => { _resultShopPresenter.RemoveAllCards(); });
        }
    }
}