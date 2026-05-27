using TMPro;
using UnityEngine;
using DG.Tweening;

public class ResultRewardUI : MonoBehaviour, IResultRewardUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Text References")]
    [SerializeField] private HighlightText goldLabel;
    [SerializeField] private HighlightText expLabel;
    [SerializeField] private TMP_Text levelUpLabel;
    
    [Header("Animation References")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private int levelUpJumpCount = 2;
    [SerializeField] private float levelUpJumpHeight = 10f;

    private bool _isLevelUp; 
    
    public void Initialize()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        goldLabel.Hide(true);
        expLabel.Hide(true);
        ToggleLevelUpIndicator(false);
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    public void UpdateUI(int gold, int exp, bool isLevelUp)
    {
        goldLabel.SetText($"· 골드 +{gold}");
        expLabel.SetText($"· 경험치 +{exp}");
        _isLevelUp = isLevelUp;
    }

    private void ToggleUI(bool isActive)
    {
        canvasGroup.DOKill();
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;

        if (isActive)
        {
            canvasGroup.DOFade(1f, animationDuration)
                       .OnComplete(() => goldLabel.Show(false)
                                                  .OnComplete(() => expLabel.Show(false)
                                                                            .OnComplete(() => ToggleLevelUpIndicator(_isLevelUp))));
        }
        else
        {
            canvasGroup.DOFade(0f, animationDuration);
            goldLabel.Hide(false);
            expLabel.Hide(false);
            ToggleLevelUpIndicator(false);
        }
    }

    private void ToggleLevelUpIndicator(bool isActive)
    {
        if (isActive)
        {
            levelUpLabel.gameObject.SetActive(true);
            levelUpLabel.transform.DOLocalJump(new Vector3(100f, -2f, 0f), levelUpJumpHeight, levelUpJumpCount, animationDuration)
                                  .SetDelay(0.5f)
                                  .SetLoops(-1, LoopType.Restart);
        }
        else
        {
            levelUpLabel.transform.DOKill();
            levelUpLabel.transform.localPosition = new Vector3(100f, -5f, 0f);
            levelUpLabel.gameObject.SetActive(false);
        }
    }
}