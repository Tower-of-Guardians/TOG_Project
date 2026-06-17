using System;
using System.Collections.Generic;
using UnityEngine;

public class CardContainer<TUI, TPresenter> where TPresenter : CardPresenter
{
    private readonly List<TUI> _cardList = new();
    private readonly Dictionary<TUI, TPresenter> _cardDict = new();

    public IReadOnlyList<TUI> CardList => _cardList;

    /// <summary>
    /// 컨테이너에 중복되지 않는 카드를 추가하고 성공 여부를 반환합니다.
    /// </summary>
    public bool Add(TUI cardUI, TPresenter cardPresenter)
    {
        if(IsExist(cardUI))
        {
            Debug.LogWarning("CardContainer: 관리 중인 카드를 추가하려는 시도가 있었습니다.");
            return false;
        }

        _cardList.Add(cardUI);
        _cardDict[cardUI] = cardPresenter;

        return true;
    }

    /// <summary>
    /// 컨테이너에서 관리 중인 카드를 제거하고 성공 여부를 반환합니다.
    /// </summary>
    public bool Remove(TUI cardUI)
    {
        if(!IsExist(cardUI))
        {
            Debug.LogWarning("CardContainer: 관리 중이지 않은 카드를 제거하려는 시도가 있었습니다.");
            return false;
        }

        _cardList.Remove(cardUI);
        _cardDict.Remove(cardUI);

        return true;
    }

    /// <summary>
    /// 컨테이너를 초기화합니다.
    /// </summary>
    public void Clear()
    {
        _cardList.Clear();
        _cardDict.Clear();
    }

    /// <summary>
    /// fromUI 카드를 toUI 카드 자리에 삽입합니다.
    /// </summary>
    /// 
    public void Insert(TUI fromUI, TUI toUI)
    {
        if(!TryGetCardIndex(fromUI, out int fromIndex))
        {
            return;
        }

        if(!TryGetCardIndex(toUI, out int toIndex))
        {
            return;
        }

        if(fromIndex < 0 || toIndex < 0 || fromIndex == toIndex)
        {
            return;
        }

        _cardList.RemoveAt(fromIndex);
        _cardList.Insert(toIndex, fromUI);
    }

    /// <summary>
    /// fromUI 카드가 toUI 카드보다 앞서는지의 여부를 반환합니다.
    /// </summary>
    public bool IsPriority(TUI fromUI, TUI toUI)
    {
        if(!TryGetCardIndex(fromUI, out int fromIndex))
        {
            return false;
        }

        if(!TryGetCardIndex(toUI, out int toIndex))
        {
            return false;
        }

        return fromIndex < toIndex;
    }

    /// <summary>
    /// 해당 카드가 관리 대상인지의 여부를 반환합니다.
    /// </summary>
    public bool IsExist(TUI cardUI)
        => _cardDict.ContainsKey(cardUI);

    /// <summary>
    /// BattleCardData를 가지고 있는 카드와 탐색 성공 여부를 반환합니다.
    /// </summary>
    public bool TryGetUI(BattleCardData battleCardData, out TUI cardUI)
    {
        cardUI = default;

        if(battleCardData == null)
        {
            Debug.LogWarning($"CardContainer: battleCardData가 null입니다.");
            return false;
        }

        foreach(TUI candidateCardUI in _cardList)
        {
            TPresenter cardPresenter = _cardDict[candidateCardUI];
            
            if(cardPresenter.BattleCardData.data.id == battleCardData.data.id)
            {
                cardUI = candidateCardUI;
                return true;
            }
        }

        Debug.LogWarning($"CardContainer: {battleCardData.data.id}에 해당하는 카드를 관리하고 있지 않습니다.");
        return false;
    }

    /// <summary>
    /// 모든 카드가 담긴 배열과 데이터 무결성 여부를 반환합니다.
    /// </summary>
    public bool TryGetAllUIs(out TUI[] cardArray)
    {
        cardArray = Array.Empty<TUI>();

        if(_cardList.Count != _cardDict.Count)
        {
            return false;
        }

        cardArray = _cardList.ToArray();

        return true;
    }

    /// <summary>
    /// 카드에 해당하는 프레젠터와 탐색 성공 여부를 반환합니다.
    /// </summary>
    public bool TryGetPresenter(TUI cardUI, out TPresenter cardPresenter)
    {
        if(!_cardDict.TryGetValue(cardUI, out cardPresenter))
        {
            Debug.LogWarning($"CardContainer: 관리 중이지 않은 카드의 프레젠터를 얻으려고 했습니다.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 카드의 인덱스와 탐색 성공 여부를 반환합니다.
    /// </summary>
    public bool TryGetCardIndex(TUI cardUI, out int index)
    {
        index = -1;

        if(!IsExist(cardUI))
        {
            Debug.LogWarning($"CardContainer: 관리 중이지 않은 카드의 인덱스를 얻으려고 했습니다.");
            return false;
        }

        index = _cardList.IndexOf(cardUI);

        return true;
    }

    /// <summary>
    /// 카드를 통해 카드의 데이터와 탐색 성공 여부를 반환합니다.
    /// </summary>
    public bool TryGetCardData(TUI cardUI, out BattleCardData battleCardData)
    {
        battleCardData = null;

        if(!IsExist(cardUI))
        {
            Debug.LogWarning($"CardContainer: 관리 중이지 않은 카드의 데이터를 얻으려고 했습니다.");
            return false;            
        }

        TPresenter cardPresenter = _cardDict[cardUI];
        battleCardData = cardPresenter.BattleCardData;

        return true;
    }

    /// <summary>
    /// 관리 중인 모든 카드의 데이터를 배열로 반환합니다.
    /// </summary>
    public BattleCardData[] GetAllDatas()
    {
        List<BattleCardData> cardDataList = new();

        foreach(TUI cardUI in _cardList)
        {
            TPresenter cardPresenter = _cardDict[cardUI];
            cardDataList.Add(cardPresenter.BattleCardData);
        }

        return cardDataList.ToArray();
    }    


}
