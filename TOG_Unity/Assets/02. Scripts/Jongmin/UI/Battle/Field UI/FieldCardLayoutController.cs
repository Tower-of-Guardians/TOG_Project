using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FieldCardLayoutController : MonoBehaviour
{
    [SerializeField] private FieldUIDesigner _fieldUIDesigner;
    [SerializeField] private GameObject _previewObject;

    private CardContainer<IFieldCardUI, FieldCardPresenter> _fieldCardContainer;
    private FieldPresenter _fieldPresenter;

    public void Construct(CardContainer<IFieldCardUI, FieldCardPresenter> fieldCardContainer,
                          FieldPresenter fieldPresenter)
    {
        _fieldCardContainer = fieldCardContainer;
        _fieldPresenter = fieldPresenter;
    }

    /// <summary>
    /// 필드 카드의 레이아웃을 재조정합니다.
    /// </summary>
    public void UpdateLayout(bool isIncludePreview, bool isAnime = true, bool isSorting = true)
    {
        IReadOnlyList<IFieldCardUI> cardUIList = _fieldCardContainer.CardList;
        int cardCount = cardUIList.Count;


        Vector2 prevPreviewPosition = cardCount > 0 ? ((cardUIList[^1] as FieldCardUI).transform as RectTransform).anchoredPosition
                                                    : CardLayoutCalculator.CalculatedFieldCardPosition(0, _fieldUIDesigner.ATKLimit, _fieldUIDesigner.Space);


        CalculateCardLayout(cardUIList, cardCount, isAnime, isSorting);
        CalculatePreview(isIncludePreview, cardCount, prevPreviewPosition);
    }

    private void CalculateCardLayout(IReadOnlyList<IFieldCardUI> cardUIList, int cardCount, bool isAnime, bool isSorting)
    {
        for(int cardIndex = 0; cardIndex < cardCount; cardIndex++)
        {
            if(_fieldPresenter.HoverCard == cardUIList[cardIndex])
            {
                continue;
            }

            Vector2 layoutPosition = CardLayoutCalculator.CalculatedFieldCardPosition(cardIndex, _fieldUIDesigner.ATKLimit, _fieldUIDesigner.Space);
            FieldCardUI concreteCardUI = cardUIList[cardIndex] as FieldCardUI;
            RectTransform cardRectTransform = concreteCardUI.transform as RectTransform;

            concreteCardUI.DOKill();

            if(isAnime || isSorting)
            {
                cardRectTransform.DOAnchorPos(layoutPosition, _fieldUIDesigner.AnimeDuration).SetEase(Ease.InOutSine);
            } 
            else
            {
                if(cardCount - cardIndex > 1)
                {
                    cardRectTransform.DOAnchorPos(layoutPosition, _fieldUIDesigner.AnimeDuration).SetEase(Ease.InOutSine);
                }
                else
                {
                    cardRectTransform.anchoredPosition = layoutPosition;
                }
            }
        }
    }

    private void CalculatePreview(bool isCalculate, int cardCount, Vector2 prevPreviewPosition)
    {
        if(!isCalculate)
        {
            return;
        }

        if(cardCount == _fieldUIDesigner.ATKLimit)
        {
            return;
        }

        RectTransform previewRectTransform = _previewObject.transform as RectTransform;
        previewRectTransform.anchoredPosition = prevPreviewPosition;

        Vector2 previewLayoutPosition = CardLayoutCalculator.CalculatedFieldCardPosition(cardCount, _fieldUIDesigner.ATKLimit, _fieldUIDesigner.Space);
        previewRectTransform.anchoredPosition = previewLayoutPosition;
    }
}