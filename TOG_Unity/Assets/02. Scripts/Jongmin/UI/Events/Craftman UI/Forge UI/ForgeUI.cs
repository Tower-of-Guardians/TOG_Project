using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ForgeUI : MonoBehaviour, IForgeUI
{
    [Header("Object References")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private CanvasGroup forgeCardObject;
    [SerializeField] private CanvasGroup upgradeButtonCanvasGroup;
    [SerializeField] private CanvasGroup cancelButtonCanvasGroup;
    
    [Header("UI References")]
    [SerializeField] private Button atkUpgradeButton;
    [SerializeField] private Button bothUpgradeButton;
    [SerializeField] private Button defUpgradeButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button closeButton;
    
    [Header("Animation References")]
    [SerializeField] private Vector3 activePosition;
    [SerializeField] private Vector3 deactivePosition;

    private Sequence activeSequenceTween;

    public void Construct(ForgePresenter forgePresenter)
    {
        atkUpgradeButton.onClick.AddListener(forgePresenter.OnClickedAtkUpgrade);
        bothUpgradeButton.onClick.AddListener(forgePresenter.OnClickedBothUpgrade);
        defUpgradeButton.onClick.AddListener(forgePresenter.OnClickedDefUpgrade);
        cancelButton.onClick.AddListener(forgePresenter.OnClickedCancel);
        closeButton.onClick.AddListener(forgePresenter.OnClickedClose);
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
    {
        ToggleUI(false);
        ToggleCloseButton(false);
    }

    public void ToggleButtonGroup(bool isActive)
        => upgradeButtonCanvasGroup.interactable = isActive;

    public void ToggleCloseButton(bool isActive)
        => closeButton.gameObject.SetActive(isActive);

    private void ToggleUI(bool isActive)
    {
        activeSequenceTween?.Kill();
        
        upgradeButtonCanvasGroup.interactable = false;
        upgradeButtonCanvasGroup.blocksRaycasts = false;
        
        cancelButtonCanvasGroup.interactable = false;
        cancelButtonCanvasGroup.blocksRaycasts = false;
        
        if (isActive)
        {
            upgradeButtonCanvasGroup.alpha = 0f;
            cancelButtonCanvasGroup.alpha = 0f;
            forgeCardObject.alpha = 0f;
            
            activeSequenceTween = DOTween.Sequence();

            activeSequenceTween.Append(rootObject.transform.DOLocalMove(activePosition, 0.5f));
            activeSequenceTween.AppendInterval(0.25f);
            
            activeSequenceTween.Append(forgeCardObject.transform.DOScale(new Vector3(1.5f, 1.5f, 1f), 0f));
            activeSequenceTween.Join(forgeCardObject.DOFade(1f, 0f));
            activeSequenceTween.Append(forgeCardObject.transform.DOScale(Vector3.one, 0.5f));
            activeSequenceTween.AppendInterval(0.25f);
            
            activeSequenceTween.Append(upgradeButtonCanvasGroup.DOFade(1f, 0.5f));
            activeSequenceTween.AppendCallback(() =>
            {
                upgradeButtonCanvasGroup.interactable = true;
                upgradeButtonCanvasGroup.blocksRaycasts = true;
            });
            activeSequenceTween.AppendInterval(0.25f);
            
            activeSequenceTween.Append(cancelButtonCanvasGroup.DOFade(1f, 0.5f));
            activeSequenceTween.AppendCallback(() =>
            {
                cancelButtonCanvasGroup.interactable = true;
                cancelButtonCanvasGroup.blocksRaycasts = true;
            });
        }
        else
        {
            activeSequenceTween = DOTween.Sequence();
            
            activeSequenceTween.Append(rootObject.transform.DOLocalMove(deactivePosition, 0.5f));
            activeSequenceTween.Join(forgeCardObject.DOFade(0f, 0.5f));
            activeSequenceTween.Join(upgradeButtonCanvasGroup.DOFade(0f, 0.5f));
            activeSequenceTween.Join(cancelButtonCanvasGroup.DOFade(0f, 0f));
            activeSequenceTween.JoinCallback(() =>
            {
                cancelButtonCanvasGroup.interactable = false;
                cancelButtonCanvasGroup.blocksRaycasts = false;
            });
        }

        activeSequenceTween.Play();
    }
}
