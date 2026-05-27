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

    private MerchantDeckInvenPresenter _merchantDeckInvenPresenter;

    public void Construct(DeckInvenPresenter deckInvenPresenter)
    {
        var merchantDeckInvenPresenter = deckInvenPresenter as MerchantDeckInvenPresenter;
        if (merchantDeckInvenPresenter == null)
        {
            return;
        }

        saleButton.onClick.AddListener(merchantDeckInvenPresenter.OnClickedSale);
        backButton.onClick.AddListener(merchantDeckInvenPresenter.OnClickedBack);
    }

    public void OpenUI()
        => ToggleUI(true);

    public void CloseUI()
        => ToggleUI(false);

    private void ToggleUI(bool isActive)
    {
        transform.DOKill();
        
        saleButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (isActive)
        {
            Sequence sequence = DOTween.Sequence();
            
            sequence.Append(transform.DOLocalMoveX(-480f, animationDuration));
            sequence.AppendCallback(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                
                saleButton.gameObject.SetActive(true);
                backButton.gameObject.SetActive(true);
            });
        }
        else
        {
            transform.DOLocalMoveX(-1960f, animationDuration);
        }
    }
}