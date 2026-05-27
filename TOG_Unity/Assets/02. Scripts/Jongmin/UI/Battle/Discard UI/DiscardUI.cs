using UnityEngine;
using UnityEngine.UI;

public class DiscardUI : MonoBehaviour, IDiscardUI
{
    [Header("Object References")]
    [SerializeField] private GameObject _discardPreviewCard;

    [Header("Button References")]
    [SerializeField] private Button _uiOpenButton;
    [SerializeField] private Button _uiCloseButton;
    [SerializeField] private Button _cardDiscardButton;

    [Header("Animator References")]
    [SerializeField] private Animator _discardAnimator;

    private static readonly int s_cachedOpenParameter = Animator.StringToHash("Open");

    public void BindPresenter(DiscardPresenter discardPresenter)
    {
        _uiOpenButton.onClick.AddListener(discardPresenter.OnClickedOpenButton);
        _uiCloseButton.onClick.AddListener(discardPresenter.OnClickedCloseButton);
        _cardDiscardButton.onClick.AddListener(discardPresenter.OnClickedDiscardButton);
    }

    public void OpenUI()
        => ToggleOpenAnimation(true);

    public void CloseUI()
        => ToggleOpenAnimation(false);

    /// <summary>
    /// 프리뷰 카드의 가시성을 토글합니다.
    /// </summary>
    public void TogglePreview(bool isActive)
    {
        _discardPreviewCard.SetActive(isActive);
        _discardPreviewCard.transform.SetAsFirstSibling();
    }

    /// <summary>
    /// 열기 버튼의 상호작용 여부를 갱신합니다.
    /// </summary>
    public void UpdateOpenButtonState(bool isOpenButtonActive)
        => _uiOpenButton.interactable = isOpenButtonActive;

    /// <summary>
    /// 카드 교체 버튼의 상호작용 여부를 갱신합니다.
    /// </summary>
    public void UpdateDiscardButtonState(bool isDiscardButtonActive)
        => _cardDiscardButton.interactable = isDiscardButtonActive;

    private void ToggleOpenAnimation(bool isOpen)
        => _discardAnimator.SetBool(s_cachedOpenParameter, isOpen);
}
