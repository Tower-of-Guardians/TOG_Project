using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public class HandCardEventController : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject _handPreviewCard;
    private HandUIDesigner _handUIDesigner;
    private HandPresenter _handPresenter;
    private CardContainer<IHandCardUI, HandCardPresenter> _handCardContainer;
    private HandCardLayoutController _handCardLayout;

    private CardDropSystem _cardDropSystem;
    private CardInfoUI _cardInfoUI;
    private TurnManager _turnManager;

    private Dictionary<IHandCardUI, HandCardEventBundle> _eventDict = new();

    [Inject]
    private void Construct(HandUIDesigner handUIDesigner,
                           HandPresenter handPresenter,
                           CardContainer<IHandCardUI, HandCardPresenter> handCardContainer,
                           HandCardLayoutController handCardLayout,
                           CardDropSystem cardDropSystem,
                           CardInfoUI cardInfoUI,
                           TurnManager turnManager)
    {
        _handUIDesigner = handUIDesigner;
        _handPresenter = handPresenter;
        _handCardContainer = handCardContainer;
        _handCardLayout = handCardLayout;

        _cardDropSystem = cardDropSystem;
        _cardInfoUI = cardInfoUI;
        _turnManager = turnManager;
    }

    /// <summary>
    /// 핸드 카드에 이벤트 리스너를 등록합니다.
    /// </summary>
    public void Subscribe(IHandCardUI cardUI)
    {
        var newBundle = new HandCardEventBundle
        {
            OnPointerEnter =    ()          => { OnPointerEnterInCard(cardUI); },
            OnPointerExit =     ()          => { OnPointerExitFromCard(); },
            OnBeginDrag =       ()          => { OnBeginDragCard(); },
            OnDrag =            (position)  => { OnDragCard(position); },
            OnEndDrag =         ()          => { OnEndDragCard(); },
            OnPointerClick =    ()          => { OnPointerClickCard(); }
        };

        _eventDict[cardUI] = newBundle;

        cardUI.OnPointerEnterAction += newBundle.OnPointerEnter;
        cardUI.OnPointerExitAction += newBundle.OnPointerExit;
        cardUI.OnBeginDragAction += newBundle.OnBeginDrag;
        cardUI.OnDragAction += newBundle.OnDrag;
        cardUI.OnEndDragAction += newBundle.OnEndDrag;
        cardUI.OnPointerClickAction += newBundle.OnPointerClick;
    }

    /// <summary>
    /// 핸드 카드로부터 이벤트 리스너를 해제합니다.
    /// </summary>
    public void Unsubscribe(IHandCardUI cardUI)
    {
        if(_eventDict.TryGetValue(cardUI, out var bundle))
        {
            cardUI.OnPointerEnterAction -= bundle.OnPointerEnter;
            cardUI.OnPointerExitAction -= bundle.OnPointerExit;
            cardUI.OnBeginDragAction -= bundle.OnBeginDrag;
            cardUI.OnDragAction -= bundle.OnDrag;
            cardUI.OnEndDragAction -= bundle.OnEndDrag;    
            cardUI.OnPointerClickAction -= bundle.OnPointerClick;        
        }
    }

    private void OnPointerEnterInCard(IHandCardUI cardUI)
    {
        if(!_handCardContainer.IsExist(cardUI))
        {
            return;
        }

        _handPresenter.HoverCard = cardUI;
        _handCardLayout.UpdateLayout();
    }

    private void OnPointerExitFromCard()
    {
        _handPresenter.HoverCard = null;
        _handCardLayout.UpdateLayout();
    }

    private void OnBeginDragCard()
    {
        if(_handPresenter.HoverCard == null)
        {
            _handPresenter.ToggleFieldPreview(false);
            _handPreviewCard.SetActive(false);
            _handCardLayout.UpdateLayout();

            return;
        }
            
        HandCardUI concreteHoverCard = _handPresenter.HoverCard as HandCardUI; 
        concreteHoverCard.transform.DOKill();

        if(_turnManager.CanAction)
        {
            _handPresenter.ToggleFieldPreview(true);
            CalculatePreviewPosition();
        }
    }

    private void OnDragCard(Vector2 position)
    {
        if(_handPresenter.HoverCard == null)
        {
            _handPresenter.ToggleFieldPreview(false);
            _handPreviewCard.SetActive(false);
            _handCardLayout.UpdateLayout();

            return;
        }

        if(!_handCardContainer.IsExist(_handPresenter.HoverCard))
        {
            _handPresenter.HoverCard = null;
            _handPreviewCard.SetActive(false);
            _handCardLayout.UpdateLayout();
            return;
        }

        HandCardUI concreteHoverCard = _handPresenter.HoverCard as HandCardUI;
        concreteHoverCard.transform.position = position;

        IHandCardUI cardUI = GetHandCardUI();
        if(cardUI != null)
        {
            if(_handCardContainer.IsExist(cardUI))
            {
                InsertInSameField(cardUI, position);
                CalculatePreviewPosition();
            }
        }
        else
        {
            RaycastResult? drop_hit = CheckField(out var _);
            IDropHandler dropHandler = drop_hit?.gameObject.GetComponent<IDropHandler>();

            if(dropHandler != null)
            {
                concreteHoverCard.transform.DOScale(0.75f, _handUIDesigner.AnimeSPD);
            }
            else
            {
                concreteHoverCard.transform.DOScale(_handUIDesigner.Scale, _handUIDesigner.AnimeSPD);
            }
        }
    }

    private void OnEndDragCard()
    {
        if(_handPresenter.HoverCard == null)
        {
            _handPresenter.ToggleFieldPreview(false);
            _handPreviewCard.SetActive(false);
            _handCardLayout.UpdateLayout();

            return;
        }

        if(!_handCardContainer.IsExist(_handPresenter.HoverCard))
        {
            _handPresenter.HoverCard = null;
            _handPresenter.ToggleFieldPreview(false);
            _handPreviewCard.SetActive(false);
            _handCardLayout.UpdateLayout();
            return;
        }

        RaycastResult? hit = CheckField(out var pointer_data);
        IDropHandler dropHandler = hit?.gameObject.GetComponent<IDropHandler>();

        if(dropHandler != null)
        {
            ExecuteEvents.Execute(hit?.gameObject, pointer_data, ExecuteEvents.dropHandler);
        }
            
        _handPresenter.ToggleFieldPreview(false);
        _handPresenter.HoverCard = null;

        _handPreviewCard.SetActive(false);
        CommitChange();
        _handCardLayout.UpdateLayout();
    }

    private void OnPointerClickCard()
    {
        if(_handPresenter.HoverCard == null || !_handCardContainer.IsExist(_handPresenter.HoverCard))
        {
            return;
        }

        if(!_handCardContainer.TryGetCardData(_handPresenter.HoverCard, out BattleCardData battleCardData))
        {
            return;
        }

        _cardInfoUI.ShowCardInfo(battleCardData.data);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if(droppedObject != null)
        {
            IDiscardCardUI discardCardUI = droppedObject.GetComponent<IDiscardCardUI>();
            if(discardCardUI != null)
            {
                _cardDropSystem.OnDropedDiscardToHand(discardCardUI);
            }

            IFieldCardUI fieldCardUI = droppedObject.GetComponent<IFieldCardUI>();
            if(fieldCardUI != null)
            {
                _cardDropSystem.OnDropedFieldToHand(fieldCardUI);
            }
        }
    }

    public void CommitChange()
    {
        IReadOnlyList<IHandCardUI> handCardList = _handCardContainer.CardList;

        for (int cardIndex = 0; cardIndex < handCardList.Count; cardIndex++)
        {
            if(!_handCardContainer.TryGetPresenter(handCardList[cardIndex], out HandCardPresenter handCardPresenter))
            {
                continue;
            }

            string cardID = handCardPresenter.CardData.id;

            if (cardIndex < GameData.Instance.handDeck.Count)
            {
                GameData.Instance.handDeck[cardIndex] = cardID;
            }
            else
            {
                GameData.Instance.handDeck.Add(cardID);
            }
        }

        while (GameData.Instance.handDeck.Count > handCardList.Count)
            GameData.Instance.handDeck.RemoveAt(GameData.Instance.handDeck.Count - 1);
    }

    private RaycastResult? CheckField(out PointerEventData pointerData)
    {
        pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        pointerData.pointerDrag = (_handPresenter.HoverCard as HandCardUI).gameObject;

        var rayHits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, rayHits);

        foreach(RaycastResult hit in rayHits)
        {
            var carHit = hit.gameObject.GetComponent<IHandCardUI>();
            if(carHit != null && _handPresenter.HoverCard != carHit)
            {
                return hit;
            }

            var fieldHandler = hit.gameObject.GetComponent<FieldCardEventController>();
            if(fieldHandler != null)
            {
                return hit;
            }

            var discardHandler = hit.gameObject.GetComponent<DiscardCardEventController>();
            if(discardHandler != null)
            {
                return hit;
            } 
        } 

        return null;
    }

    private IHandCardUI GetHandCardUI()
    {
        RaycastResult? hit = CheckField(out _);
        return hit?.gameObject.GetComponent<IHandCardUI>();
    }

    private void InsertInSameField(IHandCardUI cardUI, Vector2 position)
    {
        IHandCardUI hoverCardUI = _handPresenter.HoverCard;
        HandCardUI concreteCardUI = cardUI as HandCardUI;

        if(_handCardContainer.IsPriority(hoverCardUI, cardUI))
        {
            if(position.x >= concreteCardUI.transform.position.x)
            {
                _handCardContainer.Insert(hoverCardUI, cardUI);
                _handCardLayout.UpdateLayout(true);
            }
        }
        else
        {
            if(position.x < concreteCardUI.transform.position.x)
            {
                _handCardContainer.Insert(hoverCardUI, cardUI);
                _handCardLayout.UpdateLayout(true);
            }
        }        
    }

    private void CalculatePreviewPosition()
    {
        if(_handPresenter.HoverCard == null || !_handCardContainer.IsExist(_handPresenter.HoverCard))
        {
            _handPreviewCard.SetActive(false);
            return;
        }

        if(!_handCardContainer.TryGetCardIndex(_handPresenter.HoverCard, out int hoverCardIndex))
        {
            return;
        }

        var layoutData = CardLayoutCalculator.CalculatedHandCardTransform(hoverCardIndex,
                                                                          _handCardContainer.CardList.Count,
                                                                          _handUIDesigner.Radius,
                                                                          _handUIDesigner.Angle,
                                                                          _handUIDesigner.Depth);      
        _handPreviewCard.SetActive(true);

        RectTransform previewRectTransform = _handPreviewCard.transform as RectTransform;
        previewRectTransform.anchoredPosition = layoutData.Position;
        previewRectTransform.rotation = Quaternion.Euler(layoutData.Rotation);
        previewRectTransform.localScale = layoutData.Scale;
    }
}
