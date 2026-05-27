using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour, IInventoryUI
{
    [Header("UI references")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button toggleButton;
    [SerializeField] private TMP_Text titleLabel;
    
    [Header("Animation References")]
    [SerializeField] private InventoryTabUI inventoryTabUI;
    [SerializeField] private float animeDuration = 0.5f;

    private Sequence _toggleSequence; 

    public void Construct(InventoryPresenter inventoryPresenter)
    {
        toggleButton.onClick.AddListener(inventoryPresenter.ToggleUI);
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    public void UpdateTitle(string titleText)
    {
        titleLabel.text = titleText;
    }

    private void ToggleUI(bool isActive)
    {
        _toggleSequence?.Kill();
        
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;

        _toggleSequence = DOTween.Sequence();

        if (isActive)
        {
            _toggleSequence.Append(canvasGroup.DOFade(1f, animeDuration));
            _toggleSequence.Join((inventoryTabUI.transform as RectTransform).DOAnchorPosX(-100f, animeDuration));
        }
        else
        {
            _toggleSequence.Append((inventoryTabUI.transform as RectTransform).DOAnchorPosX(120f, animeDuration));
            _toggleSequence.Join(canvasGroup.DOFade(0f, animeDuration));
        }
    }
}
