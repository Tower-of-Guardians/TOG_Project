using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VContainer.Unity;

public class ResultShopPresenter : IInitializable, IDisposable
{
    private readonly IResultShopUI _resultShopUI;
    private readonly ResultCardFactory _resultCardFactory;
    private readonly CardContainer<IResultCardUI, ResultCardPresenter> _resultCardContainer;
    
    private List<BattleCardData> _battleCardDataList;
    private int _index = 0;

    private int _currentRefreshCost;

    private const int MinRefreshCost = 5;
    private const int MaxRefreshCost = 20;
    private const int InitialRefreshCost = 5;
    private const int GrowthRefreshCost = 5;

    public ResultShopPresenter(IResultShopUI resultShopUI,
                               ResultCardFactory resultCardFactory,
                               CardContainer<IResultCardUI, ResultCardPresenter> resultCardContainer)
    {
        _resultShopUI = resultShopUI;
        _resultCardFactory = resultCardFactory;
        _resultCardContainer = resultCardContainer;
    }

    public void Initialize()
    {
        _resultShopUI.Construct(this);
        DataCenter.Instance.playerMoneyEvent += UpdateCardsState;
        DataCenter.Instance.playerMoneyEvent += UpdateRefreshState;
    }

    public void Dispose()
    {
        DataCenter.Instance.playerMoneyEvent -= UpdateCardsState;
        DataCenter.Instance.playerMoneyEvent -= UpdateRefreshState;
    }
    

    public void InitUI()
        => _resultShopUI.Initialize();
    
    public void OpenUI()
    {
        InitRefreshCost();
        UpdateCandidateCards();
        SetRateFromData();
        
        _resultShopUI.OpenUI();
    }

    public void CloseUI()
    {
        _resultShopUI.CloseUI();
    }

    private void CreateCard()
    {
        BattleCardData battleCardData = _battleCardDataList[_index++];
        
        IResultCardUI resultCardUI = _resultCardFactory.Create();
        ResultCardPresenter resultCardPresenter = new(resultCardUI, battleCardData);

        _resultCardContainer.Add(resultCardUI, resultCardPresenter);
    }

    public void CreateAllCards()
    {
        for (int i = 0; i < 3; i++)
        {
            CreateCard();
        }
    }

    private void RemoveCard(IResultCardUI cardUI)
        => _resultCardFactory.Release(cardUI);

    public void RemoveAllCards()
    {
        foreach (IResultCardUI cardUI in _resultCardContainer.CardList)
        {
            RemoveCard(cardUI);
        }

        _resultCardContainer.Clear();
    }

    public void Refresh()
    {
        DataCenter.Instance.SetMoney(-_currentRefreshCost);
        
        RemoveAllCards();
        
        UpdateCandidateCards();
        UpdateRefreshCost();

        for (int i = 0; i < 3; i++)
        {
            CreateCard();
        }
    }

    private void SetRateFromData()
    {
        List<string> colorList = new(){ "828282", "4AA8D8", "FEFD48", "F06464" };
        List<float> rateList = GameData.Instance.GetResultPercent();

        StringBuilder builder = new();
        for (int i = 0; i < rateList.Count; i++)
        {
            builder.Append(i < rateList.Count - 1 ? $"<color=#{colorList[i]}>{rateList[i]}%</color>   "
                                                  : $"<color=#{colorList[i]}>{rateList[i]}%</color>");
        }
            
        _resultShopUI.UpdateRate(builder.ToString());
    }

    private void InitRefreshCost()
        => _currentRefreshCost = InitialRefreshCost;

    private void UpdateRefreshCost()
    {
        _currentRefreshCost += GrowthRefreshCost;
        _currentRefreshCost = Mathf.Clamp(_currentRefreshCost, MinRefreshCost, MaxRefreshCost);
    }

    private void UpdateCandidateCards()
    {
        _index = 0;
        _battleCardDataList = GameData.Instance.GetResultItems();
    }

    /// <summary>
    /// 플레이어 돈 갱신 이벤트에 연결합니다.
    /// 돈 갱신에 따라 새로고침 버튼의 상태를 갱신합니다.
    /// </summary>
    private void UpdateRefreshState(int money)
    {
        bool canRefresh = money >= _currentRefreshCost;
        _resultShopUI.UpdateRefresh($"<size=20>새로고침</size>", canRefresh);
    }

    /// <summary>
    /// 플레이어 돈 갱신 이벤트에 연결합니다.
    /// 돈 갱신에 따라 카드의 상태를 갱신합니다.
    /// </summary>
    private void UpdateCardsState(int money)
    {
        foreach (IResultCardUI cardUI in _resultCardContainer.CardList)
        {
            if (_resultCardContainer.TryGetPresenter(cardUI, out ResultCardPresenter cardPresenter))
            {
                cardPresenter.UpdateState(money);
            }
        }
    }
}
