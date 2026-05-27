using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DiscardCardLayoutController : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] private DiscardUIDesigner _discardUIDesigner;
    [SerializeField] private GameObject _discardPreviewCard;

    private DiscardPresenter _discardPresenter;
    private CardContainer<IDiscardCardUI, DiscardCardPresenter> _discardCardContainer;

    public void Construct(DiscardPresenter discardPresenter,
                          CardContainer<IDiscardCardUI, DiscardCardPresenter> discardCardContainer)
    {
        _discardPresenter = discardPresenter;
        _discardCardContainer = discardCardContainer;
    }
    
    /// <summary>
    /// 교체 카드의 레이아웃을 재조정합니다.
    /// </summary>
    public void UpdateLayout(bool isIncludePreview, bool isAnime = true, bool isSorting = true)
    {
        IReadOnlyList<IDiscardCardUI> cardUIList = _discardCardContainer.CardList;
        int realCardCount = cardUIList.Count;
        int virtualCardCount = isIncludePreview ? realCardCount + 1
                                                : realCardCount;

        if(virtualCardCount == 0)
        {
            return;
        }

        Vector2 prevPreviewPosition = realCardCount > 0 ? ((cardUIList[^1] as DiscardCardUI).transform as RectTransform).anchoredPosition
                                                        : Vector2.zero;


        CalculateCardLayout(cardUIList, realCardCount, virtualCardCount, isAnime, isSorting);
        CalculatePreview(isIncludePreview, virtualCardCount, prevPreviewPosition);
    }

    private void CalculateCardLayout(IReadOnlyList<IDiscardCardUI> cardUIList, int realCardCount, int virtualCardCount, bool isAnime, bool isSorting)
    {
        for(int cardIndex = 0; cardIndex < realCardCount; cardIndex++)
        {
            if(_discardPresenter.HoverCard == cardUIList[cardIndex])
            {
                continue;
            }

            Vector2 layoutPosition = CardLayoutCalculator.CalculatedThrowCardPosition(cardIndex, virtualCardCount, _discardUIDesigner.Space);
            DiscardCardUI concreteCardUI = cardUIList[cardIndex] as DiscardCardUI;
            RectTransform concreteCardUIRectTransform = concreteCardUI.transform as RectTransform;

            concreteCardUI.DOKill();
            if(isAnime || isSorting)
            {
                concreteCardUIRectTransform.DOAnchorPos(layoutPosition, _discardUIDesigner.AnimeDuration).SetEase(Ease.InOutSine);
            }
            else
            {
                concreteCardUIRectTransform.anchoredPosition = layoutPosition;
            }
        }
    }

    private void CalculatePreview(bool isCalculate, int cardCount, Vector2 prevPreviewPosition)
    {
        if(!isCalculate)
        {
            return;
        }

            RectTransform previewCardUIRectTransform = _discardPreviewCard.transform as RectTransform; 
            previewCardUIRectTransform.anchoredPosition = prevPreviewPosition;
            
            Vector2 previewLayoutPosition = CardLayoutCalculator.CalculatedThrowCardPosition(cardCount - 1, cardCount, _discardUIDesigner.Space);
            previewCardUIRectTransform.anchoredPosition = previewLayoutPosition;
    }
}
