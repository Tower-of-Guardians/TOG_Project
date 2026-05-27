using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldCardEventController : MonoBehaviour, IDropHandler
{
    [Header("Object References")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Transform _ownerCardRoot;
    [SerializeField] private Transform _oppositeCardRoot;
    [SerializeField] private GameObject _fieldPreviewCard;

    private FieldPresenter _ownerFieldPresenter;
    private FieldPresenter _oppositeFieldPresenter;
    private CardContainer<IFieldCardUI, FieldCardPresenter> _ownerFieldCardContainer;
    private CardContainer<IFieldCardUI, FieldCardPresenter> _oppositeFieldCardContainer;
    private FieldCardLayoutController _ownerFieldCardLayout;
    private FieldCardLayoutController _oppositeFieldCardLayout;
    private FieldCardEventController _oppositeFieldCardEvent;

    private CardDropSystem _cardDropSystem;
    private FieldUIDesigner _fieldUIDesigner;
    private List<CardData> _ownerFieldModel;

    private readonly Dictionary<IFieldCardUI, FieldCardEventBundle> _eventDict = new();

    public void Construct(FieldPresenter ownerFieldPresenter,
                          FieldPresenter oppositeFieldPresenter,
                          CardContainer<IFieldCardUI, FieldCardPresenter> ownerFieldCardContainer,
                          CardContainer<IFieldCardUI, FieldCardPresenter> oppositeFieldCardContainer, 
                          FieldCardLayoutController ownerFieldCardLayout,
                          FieldCardLayoutController oppositeFieldCardLayout,
                          FieldCardEventController oppositeFieldCardEvent,
                          CardDropSystem cardDropSystem,
                          FieldUIDesigner fieldUIDesigner,
                          List<CardData> ownerFieldModel)
    {
        _ownerFieldPresenter = ownerFieldPresenter;
        _oppositeFieldPresenter = oppositeFieldPresenter;

        _ownerFieldCardContainer = ownerFieldCardContainer;
        _oppositeFieldCardContainer = oppositeFieldCardContainer;

        _ownerFieldCardLayout = ownerFieldCardLayout;
        _oppositeFieldCardLayout = oppositeFieldCardLayout;
        _oppositeFieldCardEvent = oppositeFieldCardEvent;

        _cardDropSystem = cardDropSystem;
        _fieldUIDesigner = fieldUIDesigner;
        _ownerFieldModel = ownerFieldModel;
    }

    public void Subscribe(IFieldCardUI cardUI)
    {
        var newBundle = new FieldCardEventBundle
        {
            OnBeginDrag =   ()          => OnBeginDragCard(cardUI),
            OnDrag =        (position)  => OnDragCard(position),
            OnEndDrag =     ()          => OnEndDragCard()
        };

        _eventDict[cardUI] = newBundle;

        cardUI.OnBeginDragAction += newBundle.OnBeginDrag;
        cardUI.OnDragAction += newBundle.OnDrag;
        cardUI.OnEndDragAction += newBundle.OnEndDrag;        
    }

    public void Unsubscribe(IFieldCardUI cardUI)
    {
        if(_eventDict.TryGetValue(cardUI, out var bundle))
        {
            cardUI.OnBeginDragAction -= bundle.OnBeginDrag;
            cardUI.OnDragAction -= bundle.OnDrag;
            cardUI.OnEndDragAction -= bundle.OnEndDrag;

            _eventDict.Remove(cardUI);
        }
    }

    public void OnBeginDragCard(IFieldCardUI fieldCardUI)
    {
        _ownerFieldPresenter.HoverCard = fieldCardUI;

        MoveHoverCardToCanvas();
        UpdatePreviewPosition();

        _oppositeFieldPresenter.TogglePreview(true);
    }

    public void OnDragCard(Vector2 position)
    {
        if(_ownerFieldPresenter.HoverCard == null)
            return;

        MoveCardToMousePosition(position);

        if(!TryGetFieldCardUI(out IFieldCardUI fieldCardUI))
        {
            return;
        }

        if(_ownerFieldCardContainer.IsExist(fieldCardUI))
        {
            TryReorderWithinSameField(fieldCardUI, position);
            UpdatePreviewPosition();
        }
    }

    public void OnEndDragCard()
    {
        if(_ownerFieldPresenter.HoverCard == null)
        {
            return;
        }

        SetCardParentToRoot();
        TryInvokeDropHandler();
        
        _ownerFieldPresenter.HoverCard = null;

        _fieldPreviewCard.SetActive(false);
        _oppositeFieldPresenter.TogglePreview(false);

        SyncDataWithContainer();
        _oppositeFieldCardEvent.SyncDataWithContainer();

        _ownerFieldCardLayout.UpdateLayout(false);
        _oppositeFieldCardLayout.UpdateLayout(false);
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

        _cardDropSystem.OnDropedHandToField(handCardUI, _ownerFieldPresenter.IsAtk);
    }

    /// <summary>
    /// 현재 필드 컨테이너의 카드 순서를 게임 데이터 리스트에 동기화합니다.
    /// </summary>
    public void SyncDataWithContainer()
    {
        IReadOnlyList<IFieldCardUI> cardUIList = _ownerFieldCardContainer.CardList;
        int cardCount = cardUIList.Count;

        for(int cardIndex = 0; cardIndex < cardCount; cardIndex++)
        {
            if(!_ownerFieldCardContainer.TryGetPresenter(cardUIList[cardIndex], out FieldCardPresenter fieldCardPresenter))
            {
                continue;
            }

            if(cardIndex < _ownerFieldModel.Count)
            {
                _ownerFieldModel[cardIndex] = fieldCardPresenter.CardData;
            }
            else
            {
                _ownerFieldModel.Add(fieldCardPresenter.CardData);
            }
        }

        while(_ownerFieldModel.Count > cardCount)
        {
            _ownerFieldModel.RemoveAt(_ownerFieldModel.Count - 1);
        }
    }

    private RaycastResult? CheckField(out PointerEventData pointerData)
    {
        FieldCardUI concreteHoverCardUI = _ownerFieldPresenter.HoverCard as FieldCardUI;
        GameObject hoverCardObject = concreteHoverCardUI.gameObject;

        pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        pointerData.pointerDrag = hoverCardObject;

        List<RaycastResult> rayHitList = new();
        EventSystem.current.RaycastAll(pointerData, rayHitList);

        foreach(var hit in rayHitList)
        {
            var field = hit.gameObject.GetComponent<FieldCardEventController>();
            if(field != null && field != this)
            {
                return hit;
            }

            var fieldCardUI = hit.gameObject.GetComponent<IFieldCardUI>();
            if(fieldCardUI != null && fieldCardUI != _ownerFieldPresenter.HoverCard)
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
        if(!_ownerFieldCardContainer.TryGetCardIndex(_ownerFieldPresenter.HoverCard, out int cardIndex))
        {
            return;
        }

        _fieldPreviewCard.SetActive(true);

        RectTransform fieldCardRectTransform = _fieldPreviewCard.transform as RectTransform; 
        fieldCardRectTransform.anchoredPosition = CardLayoutCalculator.CalculatedFieldCardPosition(cardIndex,
                                                                                                   _fieldUIDesigner.ATKLimit,
                                                                                                   _fieldUIDesigner.Space);
    }

    /// <summary>
    /// 현재 드래그 중인 카드를 최상위 캔버스로 옮겨 자유롭게 이동할 수 있도록 설정합니다.
    /// </summary>
    private void MoveHoverCardToCanvas()
    {
        FieldCardUI concreteHoverCardUI = _ownerFieldPresenter.HoverCard as FieldCardUI;
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
    private void MoveCardToMousePosition(Vector2 mousePosition)
    {
        FieldCardUI concreteHoverCardUI = _ownerFieldPresenter.HoverCard as FieldCardUI;
        concreteHoverCardUI.transform.position = mousePosition;
    }

    /// <summary>
    /// 현재 커서 아래에 있는 필드 카드를 탐색하고 성공 여부를 반환합니다.
    /// </summary>
    private bool TryGetFieldCardUI(out IFieldCardUI fieldCardUI)
    {
        fieldCardUI = null;

        RaycastResult? cardHit = CheckField(out var _);
        if(cardHit == null)
        {
            return false;
        }

        fieldCardUI = cardHit.Value.gameObject.GetComponent<IFieldCardUI>(); 
        if(fieldCardUI == null)
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 같은 필드 내에서 드래그 중인 카드와 대상 카드의 상대 위치를 비교하여 순서를 재배치하고 레이아웃을 갱신합니다.
    /// </summary>
    private void TryReorderWithinSameField(IFieldCardUI targetCardUI, Vector2 pointerPosition)
    {
        IFieldCardUI hoverCardUI = _ownerFieldPresenter.HoverCard;
        FieldCardUI concreteFieldCardUI = targetCardUI as FieldCardUI;

        if(_ownerFieldCardContainer.IsPriority(hoverCardUI, targetCardUI))
        {
            if(pointerPosition.x >= concreteFieldCardUI.transform.position.x)
            {
                _ownerFieldCardContainer.Insert(hoverCardUI, targetCardUI);
                _ownerFieldCardLayout.UpdateLayout(false);
            }
        }
        else
        {
            if(pointerPosition.x < concreteFieldCardUI.transform.position.x)
            {
                _ownerFieldCardContainer.Insert(hoverCardUI, targetCardUI);
                _ownerFieldCardLayout.UpdateLayout(false);
            }
        }        
    }

    /// <summary>
    /// 현재 드래그 중인 카드를 반대편 필드로 이동시키면서 이벤트 구독과 컨테이너 소속을 함께 갱신합니다.
    /// </summary>
    private void MoveHoverCardToOppositeField()
    {
        IFieldCardUI hoverCardUI = _ownerFieldPresenter.HoverCard;
        if(!_ownerFieldCardContainer.TryGetPresenter(hoverCardUI, out FieldCardPresenter hoverCardPresenter))
        {
            return;
        }

        Unsubscribe(hoverCardUI);
        _oppositeFieldCardEvent.Subscribe(hoverCardUI);

        hoverCardPresenter.ToggleLock();

        _oppositeFieldCardContainer.Add(hoverCardUI, hoverCardPresenter);
        _ownerFieldCardContainer.Remove(hoverCardUI);
    }

    /// <summary>
    /// 반대편 필드 영역이 히트되면 카드를 반대편 필드로 이동시키고 성공 여부를 반환합니다.
    /// </summary>
    private bool TryInsertInOppositeFieldWithField()
    {
        RaycastResult? fieldHit = CheckField(out var _);

        FieldCardEventController field = fieldHit?.gameObject.GetComponent<FieldCardEventController>();
        if(field != null && field != this)
        {
            MoveHoverCardToOppositeField();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 반대편 필드의 카드 영역이 히트되면 카드를 반대편 필드로 이동시키고 성공 여부를 반환합니다.
    /// 필드 영역을 카드가 가릴 가능성을 생각하는 보조 메서드입니다.
    /// </summary>
    private bool TryInsertInOppositeFieldWithCard()
    {
        RaycastResult? cardHit = CheckField(out var _);

        IFieldCardUI fieldCardUI = cardHit?.gameObject.GetComponent<IFieldCardUI>();
        if(fieldCardUI != null && !_ownerFieldCardContainer.IsExist(fieldCardUI))
        {
            MoveHoverCardToOppositeField();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 드랍한 카드가 배치될 최종 필드를 기준으로 부모를 재설정하고 위치를 보정합니다.
    /// </summary>
    private void SetCardParentToRoot()
    {
        bool fieldFlag = TryInsertInOppositeFieldWithField();
        bool cardFlag = TryInsertInOppositeFieldWithCard();

        FieldCardUI hoverCardUI = _ownerFieldPresenter.HoverCard as FieldCardUI;

        Vector3 hoverCardWorldPosition = hoverCardUI.transform.position;
        Transform finalHoverCardRoot = fieldFlag || cardFlag ? _oppositeCardRoot
                                                             : _ownerCardRoot;

        hoverCardUI.transform.SetParent(finalHoverCardRoot, false);

        Vector3 hoverCardLocalPosition = hoverCardUI.transform.parent.InverseTransformPoint(hoverCardWorldPosition);
        hoverCardUI.transform.localPosition = hoverCardLocalPosition;
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