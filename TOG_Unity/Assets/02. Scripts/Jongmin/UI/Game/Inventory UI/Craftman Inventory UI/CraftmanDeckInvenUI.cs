using UnityEngine;
using DG.Tweening;

public class CraftmanDeckInvenUI : MonoBehaviour, IDeckInvenUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation References")]
    [SerializeField] private float animationDuration = 0.5f;
    
    public void Construct(DeckInvenPresenter deckInvenPresenter)
    {}

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
