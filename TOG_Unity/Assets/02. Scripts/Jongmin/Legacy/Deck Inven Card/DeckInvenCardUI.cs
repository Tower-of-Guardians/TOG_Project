using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckInvenCardUI : CardUI, 
                               IDeckInvenCardUI, 
                               IPointerEnterHandler, 
                               IPointerExitHandler, 
                               IPointerClickHandler 
{
    [Header("UI References")]
    [SerializeField] private Image selectedImage;
    
    private DeckInvenCardPresenter _deckInvenCardPresenter;
    
    public void Construct(DeckInvenCardPresenter deckInvenCardPresenter)
        => _deckInvenCardPresenter = deckInvenCardPresenter;
    
    private void OnDisable()
        => _deckInvenCardPresenter?.RequestDeselect();

    public void ShowHighlight(bool isActive)
        => selectedImage.gameObject.SetActive(isActive);

    public virtual void OnPointerEnter(PointerEventData eventData)
        => _deckInvenCardPresenter.OnPointerEnter();
    
    public virtual void OnPointerExit(PointerEventData eventData)
        => _deckInvenCardPresenter.OnPointerExit();
    
    public virtual void OnPointerClick(PointerEventData eventData)
        => _deckInvenCardPresenter.OnPointerClick();
}