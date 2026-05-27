using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DiscardCardEventController : MonoBehaviour, IDropHandler
{
    [Header("Object References")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Transform _cardRoot;
    [SerializeField] private GameObject _discardPreviewCardUI;

    private DiscardUIDesigner _discardUIDesigner;
    private DiscardPresenter _discardPresenter;
    private CardContainer<IDiscardCardUI, DiscardCardPresenter> _discardCardContainer;
    private DiscardCardLayoutController _discardCardLayout;

    private CardDropSystem _cardDropSystem;

    private Dictionary<IDiscardCardUI, DiscardCardEventBundle> _eventDict = new();

    public void Construct(DiscardUIDesigner discardUIDesigner,
                          DiscardPresenter discardPresenter,
                          CardContainer<IDiscardCardUI, DiscardCardPresenter> discardCardContainer,
                          DiscardCardLayoutController discardCardLayout,
                          CardDropSystem cardDropSystem)
    {
        _discardUIDesigner = discardUIDesigner;
        _discardPresenter = discardPresenter;
        _discardCardContainer = discardCardContainer;
        _discardCardLayout = discardCardLayout;

        _cardDropSystem = cardDropSystem;
    }

    public void Subscribe(IDiscardCardUI cardUI)
    {
        var newBundle = new DiscardCardEventBundle
        {
            OnBeginDrag =   ()          => OnBeginDragCard(cardUI),
            OnDrag =        (position)  => OnDragCard(position),
            OnEndDrag =     ()          => OnEndDragCard(),
        };

        _eventDict[cardUI] = newBundle;

        cardUI.OnBeginDragAction += newBundle.OnBeginDrag;
        cardUI.OnDragAction += newBundle.OnDrag;
        cardUI.OnEndDragAction += newBundle.OnEndDrag;
    }

    public void Unsubscribe(IDiscardCardUI cardUI)
    {
        if(_eventDict.TryGetValue(cardUI, out var bundle))
        {
            cardUI.OnBeginDragAction -= bundle.OnBeginDrag;
            cardUI.OnDragAction -= bundle.OnDrag;
            cardUI.OnEndDragAction -= bundle.OnEndDrag;

            _eventDict.Remove(cardUI);            
        }
    }

    public void OnBeginDragCard(IDiscardCardUI cardUI)
    {
        _discardPresenter.HoverCard = cardUI;

        MoveHoverCardToCanvas();
        UpdatePreviewPosition();
    }

    public void OnDragCard(Vector2 pointerPosition)
    {
        if(_discardPresenter.HoverCard == null)
        {
            return;
        }

        MoveCardToMousePosition(pointerPosition);

        if(!TryGetDiscardCardUI(out IDiscardCardUI discardCardUI))
        {
            return;
        }

        if(discardCardUI != null)
        {
            TryReorderWithinSameField(discardCardUI, pointerPosition);
            UpdatePreviewPosition();
        }
    }

    public void OnEndDragCard()
    {
        if(_discardPresenter.HoverCard == null)
        {
            return;
        }

        SetCardParentToRoot();
        TryInvokeDropHandler();

        _discardPresenter.HoverCard = null;

        _discardCardLayout.UpdateLayout(false);
        _discardPreviewCardUI.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedObject = eventData.pointerDrag;
        if(droppedObject == null)
        {
            return;
        }

        IHandCardUI handCardUI = droppedObject.GetComponent<IHandCardUI>();
        if(handCardUI == null)
        {
            return;
        }

        _cardDropSystem.OnDropedHandToDiscard(handCardUI);
    }

    private RaycastResult? CheckField(out PointerEventData pointerData)
    {
        DiscardCardUI concreteHoverCardUI = _discardPresenter.HoverCard as DiscardCardUI;
        GameObject hoverCardObject = concreteHoverCardUI.gameObject;

        pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        pointerData.pointerDrag = hoverCardObject;

        List<RaycastResult> rayHitList = new();
        EventSystem.current.RaycastAll(pointerData, rayHitList);

        foreach(var hit in rayHitList)
        {
            var discardCardUI = hit.gameObject.GetComponent<IDiscardCardUI>();
            if(discardCardUI != null && discardCardUI != _discardPresenter.HoverCard)
            {
                return hit;
            }

            var dropHandler = hit.gameObject.GetComponent<IDropHandler>();
            if(dropHandler != null)
            {
                return hit;
            }
        }

        return null;
    }

   /// <summary>
    /// 현재 드래그 중인 카드의 인덱스를 기준으로 프리뷰 카드의 위치를 계산하여 반영합니다.
    /// </summary>
    private void UpdatePreviewPosition()
    {
        if(!_discardCardContainer.TryGetCardIndex(_discardPresenter.HoverCard, out int cardIndex))
        {
            return;
        }

        _discardPreviewCardUI.SetActive(true);

        RectTransform discardCardRectTransform = _discardPreviewCardUI.transform as RectTransform;
        discardCardRectTransform.anchoredPosition = CardLayoutCalculator.CalculatedThrowCardPosition(cardIndex,
                                                                                                     _discardCardContainer.CardList.Count,
                                                                                                     _discardUIDesigner.Space);
    }

    /// <summary>
    /// 현재 드래그 중인 카드를 최상위 캔버스로 옮겨 자유롭게 이동할 수 있도록 설정합니다.
    /// </summary>
    private void MoveHoverCardToCanvas()
    {
        DiscardCardUI concreteHoverCardUI = _discardPresenter.HoverCard as DiscardCardUI;
        if(concreteHoverCardUI == null)
        {
            return;
        }

        concreteHoverCardUI.transform.DOKill();
        concreteHoverCardUI.transform.SetParent(_canvas.transform, false);
    }

    /// <summary>
    /// 드래그 중인 카드를 마우스 위치로 이동시킵니다.
    /// </summary>
    private void MoveCardToMousePosition(Vector2 pointerPosition)
    {
        DiscardCardUI concreteHoverCardUI = _discardPresenter.HoverCard as DiscardCardUI;
        concreteHoverCardUI.transform.position = pointerPosition;
    }

    /// <summary>
    /// 현재 커서 아래에 있는 교체 카드를 탐색하고 성공 여부를 반환합니다.
    /// </summary>
    private bool TryGetDiscardCardUI(out IDiscardCardUI discardCardUI)
    {
        discardCardUI = null;

        RaycastResult? cardHit = CheckField(out var _);
        if(cardHit == null)
        {
            return false;
        }

        discardCardUI = cardHit.Value.gameObject.GetComponent<IDiscardCardUI>();
        if(discardCardUI == null)
        {
            return false;
        }

        return true;        
    }

    /// <summary>
    /// 같은 필드 내에서 드래그 중인 카드와 대상 카드의 상대 위치를 비교하여 순서를 재배치하고 레이아웃을 갱신합니다.
    /// </summary>
    private void TryReorderWithinSameField(IDiscardCardUI targetCardUI, Vector2 pointerPosition)
    {
        IDiscardCardUI hoverCardUI = _discardPresenter.HoverCard;
        DiscardCardUI concreteDiscardCardUI = targetCardUI as DiscardCardUI;

        if(_discardCardContainer.IsPriority(hoverCardUI, targetCardUI))
        {
            if(pointerPosition.x >= concreteDiscardCardUI.transform.position.x)
            {
                _discardCardContainer.Insert(hoverCardUI, targetCardUI);
                _discardCardLayout.UpdateLayout(false);
            }
        }
        else
        {
            if(pointerPosition.x < concreteDiscardCardUI.transform.position.x)
            {
                _discardCardContainer.Insert(hoverCardUI, targetCardUI);
                _discardCardLayout.UpdateLayout(false);
            }
        }        
    }

    /// <summary>
    /// 현재 드래그 중인 카드를 필드로 부모를 재설정합니다.
    /// </summary>
    private void SetCardParentToRoot()
    {
        DiscardCardUI hoverCardUI = _discardPresenter.HoverCard as DiscardCardUI;
        hoverCardUI.transform.SetParent(_cardRoot, false);
    }

    /// <summary>
    /// 현재 마우스 위치의 드롭 대상이 HandUI이면 드롭 이벤트를 발행하고 성공 여부를 반환합니다.
    /// </summary>
    private bool TryInvokeDropHandler()
    {
        RaycastResult? handHit = CheckField(out PointerEventData pointerData);
        if(handHit == null)
        {
            return false;
        }

        HandUI handUI = handHit.Value.gameObject.GetComponent<HandUI>();
        if(handUI == null)
        {
            return false;
        }

        ExecuteEvents.Execute(handHit.Value.gameObject,
                              pointerData,
                              ExecuteEvents.dropHandler);
        return true;
    }
}
