using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MerchantInventoryUI : MonoBehaviour, IDeckInvenUI
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button saleButton;
    [SerializeField] private Button backButton;
    
    [Header("Animation Duration")]
    [SerializeField] private float animationDuration;

    private Tween _moveTween;

    public void Construct(DeckInvenPresenter deckInvenPresenter)
    {
        var merchantDeckInvenPresenter = deckInvenPresenter as MerchantDeckInvenPresenter;
        if (merchantDeckInvenPresenter == null)
        {
            return;
        }

        saleButton.onClick.AddListener(merchantDeckInvenPresenter.OnClickedSale);
        backButton.onClick.AddListener(merchantDeckInvenPresenter.OnClickedBack);

        Vector3 position = transform.localPosition;
        position.x = -1960f;
        transform.localPosition = position;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        saleButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    private void ToggleUI(bool isActive)
    {
        _moveTween?.Kill();
        
        saleButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (isActive)
        {
            canvasGroup.alpha = 1f;
            _moveTween = transform.DOLocalMoveX(-480f, animationDuration).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                
                saleButton.gameObject.SetActive(true);
                backButton.gameObject.SetActive(true);
            });
        }
        else
        {
            _moveTween = transform.DOLocalMoveX(-1960f, animationDuration)
                .OnComplete(() => canvasGroup.alpha = 0f);
        }
    }

    private void OnDisable()
    {
        _moveTween?.Kill();
        _moveTween = null;
    }
}
