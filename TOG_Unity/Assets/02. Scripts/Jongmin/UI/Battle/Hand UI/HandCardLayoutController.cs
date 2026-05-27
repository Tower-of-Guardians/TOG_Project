using DG.Tweening;
using UnityEngine;

public class HandCardLayoutController : MonoBehaviour
{
    [SerializeField] private HandUIDesigner _handUIDesigner;

    private CardContainer<IHandCardUI, HandCardPresenter> _handCardContainer;
    private HandPresenter _handPresenter;

    public void Construct(CardContainer<IHandCardUI, HandCardPresenter> handCardContainer,
                          HandPresenter handPresenter)
    {
        _handCardContainer = handCardContainer;
        _handPresenter = handPresenter;
    }

    /// <summary>
    /// 핸드 카드의 레이아웃을 재조정합니다.
    /// </summary>
    public void UpdateLayout(bool isDragging = false)
    {
        int cardCount = _handCardContainer.CardList.Count;
        if(cardCount <= 0)
        {
            return;
        }

        for(int cardIndex = 0; cardIndex < cardCount; cardIndex++)
        {
            IHandCardUI cardUI = _handCardContainer.CardList[cardIndex];

            if(isDragging && _handPresenter.HoverCard == cardUI)
                continue;

            var targetTransform = CardLayoutCalculator.CalculatedHandCardTransform(cardIndex,
                                                                                   cardCount,
                                                                                   _handUIDesigner.Radius,
                                                                                   _handUIDesigner.Angle,
                                                                                   _handUIDesigner.Depth);

            ApplyHoverEffect(targetTransform, cardUI, cardIndex);
            AnimateCardTransform(targetTransform, cardUI);
        }

        if (_handPresenter.HoverCard == null)
            RebuildSiblingOrder();
    }

    private void RebuildSiblingOrder()
    {
        int cardCount = _handCardContainer.CardList.Count;
        if(cardCount <= 0)
        {
            return;
        }

        for(int cardIndex = 0; cardIndex < cardCount; cardIndex++)
        {
            HandCardUI concreteCardUI = _handCardContainer.CardList[cardIndex] as HandCardUI;
            concreteCardUI.transform.SetSiblingIndex(cardIndex);
        } 
    }

    private void ApplyHoverEffect(CardLayoutData targetTransform,
                                  IHandCardUI cardUI,
                                  int cardIndex)
    {
        if(_handPresenter.HoverCard != null && !_handCardContainer.IsExist(_handPresenter.HoverCard))
        {
            _handPresenter.HoverCard = null;
            return;
        }

        if (_handPresenter.HoverCard == cardUI)
        {
            targetTransform.Scale = Vector3.one * _handUIDesigner.Scale;
            targetTransform.Rotation = Vector3.zero;

            HandCardUI concreteCardUI = cardUI as HandCardUI; 
            concreteCardUI.transform.SetAsLastSibling();
        }
        else if (_handPresenter.HoverCard != null)
        {
            if(_handCardContainer.TryGetCardIndex(_handPresenter.HoverCard, out int hoverCardIndex))
            {
                float offset = cardIndex < hoverCardIndex ? -_handUIDesigner.Strength
                                                          :  _handUIDesigner.Strength;

                targetTransform.Position.x += offset;
            }
        }
    }

    private void AnimateCardTransform(CardLayoutData targetTransform,
                                      IHandCardUI cardUI)
    {
        HandCardUI concreteCardUI = cardUI as HandCardUI; 

        concreteCardUI.transform.DOKill();
        concreteCardUI.transform.DOLocalMove(new Vector3(targetTransform.Position.x, 
                                                         cardUI == _handPresenter.HoverCard ? _handUIDesigner.HoverY 
                                                                                            : targetTransform.Position.y, 
                                                         targetTransform.Position.z), 
                                                         _handUIDesigner.AnimeSPD);
        concreteCardUI.transform.DOLocalRotate(targetTransform.Rotation, _handUIDesigner.AnimeSPD).SetEase(Ease.OutBack);
        concreteCardUI.transform.DOScale(targetTransform.Scale, _handUIDesigner.AnimeSPD).SetEase(Ease.OutBack);
    }
}
