using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopDispenser : MonoBehaviour
{
    private List<ShopCardPresenter> _shopCardPresenterList = new();
    private PotionCardPresenter _potionCardPresenter;

    public event Action OnPurchasedAnyItem;

    public void Inject(List<ShopCardPresenter> shopCardPresenterList,
                       PotionCardPresenter potionCardPresenter)
    {
        _shopCardPresenterList = shopCardPresenterList;
        _potionCardPresenter = potionCardPresenter;
    }

    public bool Initialize()
    {
        if (!TryGetRandomCards(out List<BattleCardData> battleCardDataList))
        {
            return false;
        }

        for (int i = 0; i < _shopCardPresenterList.Count; i++)
        {
            _shopCardPresenterList[i].Inject(battleCardDataList[i]);
        }

        _potionCardPresenter?.Initialize();
        return true;
    }

    public void Alert()
        => OnPurchasedAnyItem?.Invoke();

    private bool TryGetRandomCards(out List<BattleCardData> results)
    {
        results = new List<BattleCardData>(_shopCardPresenterList.Count);
        ResultPercentData resultPercent = null;
        DataCenter.Instance.GetResultPercentData(DataCenter.Instance.playerstate.level + 2, (data) =>
        {
            resultPercent = data;
        });

        if (resultPercent == null || resultPercent.percent == null || _shopCardPresenterList.Count == 0)
        {
            Debug.LogError("상점의 카드 확률 데이터 또는 판매 슬롯이 준비되어 있지 않습니다.", this);
            return false;
        }

        var cardsByGrade = new List<CardData>[resultPercent.percent.Count];
        foreach (string cardId in DataCenter.random_card_datas)
        {
            if (!DataCenter.card_datas.TryGetValue(cardId, out CardData cardData) || cardData == null)
            {
                continue;
            }

            int gradeIndex = cardData.grade - 1;
            if (gradeIndex < 0 || gradeIndex >= cardsByGrade.Length || resultPercent.percent[gradeIndex] <= 0f)
            {
                continue;
            }

            cardsByGrade[gradeIndex] ??= new List<CardData>();
            cardsByGrade[gradeIndex].Add(cardData);
        }

        float totalWeight = 0f;
        int lastAvailableGrade = -1;
        for (int i = 0; i < cardsByGrade.Length; i++)
        {
            if (cardsByGrade[i] == null)
            {
                continue;
            }

            totalWeight += resultPercent.percent[i];
            lastAvailableGrade = i;
        }

        if (lastAvailableGrade < 0 || totalWeight <= 0f)
        {
            Debug.LogError("상점 확률에 맞는 판매 가능 카드가 없습니다.", this);
            return false;
        }

        for (int i = 0; i < _shopCardPresenterList.Count; i++)
        {
            float roll = UnityEngine.Random.Range(0f, totalWeight);
            int selectedGrade = lastAvailableGrade;
            for (int gradeIndex = 0; gradeIndex < cardsByGrade.Length; gradeIndex++)
            {
                if (cardsByGrade[gradeIndex] == null)
                {
                    continue;
                }

                roll -= resultPercent.percent[gradeIndex];
                if (roll < 0f)
                {
                    selectedGrade = gradeIndex;
                    break;
                }
            }

            List<CardData> candidates = cardsByGrade[selectedGrade];
            CardData selectedCard = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            results.Add(new BattleCardData { data = Instantiate(selectedCard) });
        }

        return true;
    }
}
