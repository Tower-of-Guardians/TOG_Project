using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MerchantUI : MonoBehaviour, IMerchantUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;
    
    [Header("Animation References")]
    [SerializeField] private float animationDuration = 0.5f;

    public void Construct(MerchantPresenter merchantPresenter)
    {
        closeButton.onClick.AddListener(merchantPresenter.CloseUI);
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    private void ToggleUI(bool isActive)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(isActive ? 1f : 0f, animationDuration);
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;
    }
}
