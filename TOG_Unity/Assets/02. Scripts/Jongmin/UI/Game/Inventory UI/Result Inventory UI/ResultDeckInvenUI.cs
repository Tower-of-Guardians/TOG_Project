using UnityEngine;
using DG.Tweening;

public class ResultDeckInvenUI : MonoBehaviour, IDeckInvenUI
{
    [Header("Object References")]
    [SerializeField] private GameObject rootObject;
    
    [Header("Animation References")]
    [SerializeField] private Vector3 activePosition;
    [SerializeField] private Vector3 deactivePosition;
    [SerializeField] private float animationDuration = 0.5f;
    
    private DeckInvenPresenter _deckInvenPresenter;

    public void Construct(DeckInvenPresenter deckInvenPresenter)
        => _deckInvenPresenter = deckInvenPresenter;

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    private void ToggleUI(bool isActive)
    {
        rootObject.transform.DOKill();
        rootObject.transform.DOLocalMove(isActive ? activePosition : deactivePosition, animationDuration).SetEase(Ease.OutQuad);
    }
}