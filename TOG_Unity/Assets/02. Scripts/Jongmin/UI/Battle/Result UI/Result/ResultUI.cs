using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResultUI : MonoBehaviour, IResultUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CloseButton closeButton;
    
    [Header("Animation References")]
    [SerializeField] private float animationDuration;

    public void Construct(ResultPresenter resultPresenter)
        => closeButton.Bind(resultPresenter.CloseUI);

    /// <summary>
    /// DOTween 애니메이션 상태를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    public void ShowCloseButton()
        => closeButton.Show();
    
    private void ToggleUI(bool isActive)
    {
        canvasGroup.DOKill();
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;
        canvasGroup.DOFade(isActive ? 1f : 0f, animationDuration);
    }
}
